# 2. Error response contract

**Date:** 2026-09-22
**Status:** Accepted

## Context

An unhandled exception must still end in a well-formed HTTP response. ASP.NET Core provides the pieces — `AddProblemDetails()`, `IProblemDetailsService`, and RFC 9457 as the body format — so *how* to write the response was never the hard question. The hard question is *what the response may contain*.

The response is read by strangers. Anything in it is public: an exception message can carry SQL, a file path, or an internal service name, and a stack trace describes the code to whoever triggers it. At the same time, a user who hits an error must be able to report something that lets us find that exact failure in the logs. The contract has to satisfy both: tell the client nothing about our internals, and still give it a handle we can search on.

Two categories of failure exist in the application today, and they are not equally serious:

- **Unexpected exceptions** — a bug or a failed dependency. This is a server fault.
- **Client-cancelled requests** — the client disconnected, so the request's `CancellationToken` fired and an `OperationCanceledException` surfaced. Nothing is broken; nobody is waiting for an answer.

## Decision

A global exception-handling middleware catches everything that escapes the pipeline and applies one contract.

**Unexpected exceptions** return `500` with `Content-Type: application/problem+json` and exactly these fields:

| Field | Value | Why it is safe to send |
|---|---|---|
| `type` | `about:blank` | Declares that the error has no meaning beyond its HTTP status code. It names nothing in the application. |
| `title` | `Internal Server Error` | The HTTP reason phrase for 500, as RFC 9457 requires when `type` is `about:blank`. Generic by definition. |
| `status` | `500` | Tells the client only that the server failed to process the request. |
| `detail` | a fixed, generic sentence | Deliberately the same for every occurrence, so no exception message, stack trace, SQL, path or service name can reach it. |
| `traceId` | the request's trace identifier | An opaque correlation value. It describes nothing about the failure, but it locates the failure in our logs. |

The full exception, including its type and stack trace, is written to the log at `Error` level together with the same trace identifier.

**The body is identical in every environment.** There is no Development branch that adds exception details to the response.

**`traceId` is owned by the framework, not by us.** Inspecting the IL of `DefaultProblemDetailsWriter.WriteAsync` (and of MVC's `DefaultProblemDetailsFactory`) shows it writes `Extensions["traceId"]` through the dictionary indexer — an unconditional overwrite — so a value supplied by the caller is replaced; an experiment confirmed it. The framework takes the value from `Activity.Current?.Id` and falls back to `HttpContext.TraceIdentifier`. Our log lines use the same expression, so the id in the body and the id in the log are computed identically and cannot drift apart.

**Client-cancelled requests** are logged at `Information`, receive status `499`, and get no body. `499` is a log-only status: the client has already gone and never receives it. It exists so that logs and metrics can tell "the client left" apart from "we failed".

**Log output differs by environment; the response does not.** In Development, logs are read by a person in a terminal, so the console uses the `simple` formatter with scopes off and the trace id is written into the message template of the error and request-summary lines. In Production, logs are read by a machine, so the console uses the `json` formatter with scopes on and every entry carries the trace id as a searchable field. This is configuration only, in `appsettings.json` and `appsettings.Development.json`.

## Consequences

**Gained.** Nothing internal can leak through an error response, because nothing internal is ever put into one — the generic `detail` removes the possibility rather than filtering for it. There is one contract to reason about instead of two, and no environment variable whose misconfiguration on a server would start sending stack traces to strangers. A user can report the `traceId`, and that value finds both the request-summary line and the exception line. Relying on the framework for `traceId` means one less value to maintain, and it already handles the case where `Activity.Current` is null.

**Given up.** A developer never sees the exception in the response, even on their own machine; they read it in the log, which has both the stack trace and the trace id. `detail` carries no occurrence-specific information, because we have no defined set of messages that are safe to show for unexpected failures; the trace id identifies the occurrence instead.

The pipeline order has a cost as well. The request logger wraps the exception handler, so it records the real `500` rather than the default `200`. But the handler ends the request without rethrowing, so the exception never reaches the logger. No single log line holds everything: the exception line has the stack trace, the request-summary line has the path, duration and status, and the trace id is what joins them.

In Development the trace id is added to each message template by hand. A new log call that forgets `{TraceId}` produces a line that cannot be correlated there. Production is not affected, because scopes attach the id to every entry automatically. The two forms also differ: the body carries the full W3C id (`00-<trace-id>-<span-id>-00`), while scopes carry only the 32-character trace id. A text search for the 32 characters matches both.

## Revisit when

A third category of exception appears that needs its own ProblemDetails — the first domain exception type, such as a business rule violation that should become a `404` or a `409`. The two categories above fit in one middleware with a single condition. A third turns that condition into a mapping, and that is the point to move to the `IExceptionHandler` chain, where each category is handled by its own class and the first handler to accept the exception ends the chain.

Separately, when a log store is introduced for Development as well, the `{TraceId}` placeholders in message templates become redundant with scopes and should be removed.
