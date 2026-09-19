# 📋 Task 2.2 — Global exception handling with ProblemDetails

**Status:** 🔄 In progress
**Branch:** `feat/exception-handling` off **`main`**

## Goal
One place in the application turns any unhandled exception into a consistent, safe HTTP response. The client gets a well-formed **RFC 9457 ProblemDetails** document. The client never gets a stack trace. And every error response carries an id that you can find in your logs.

## Why this exists / when NOT to use it
- **Why:** without this, an unhandled exception produces whatever the host decides — a blank 500, or in Development a full stack trace including file paths and connection strings. Neither is an API. Clients need one predictable error shape they can parse, and support needs a way to connect "it broke at 3pm" to a line in your logs.
- **When NOT to:** do not use it to handle *expected* outcomes. "This question does not exist" is a 404 your code should return directly. Exceptions are for the unexpected. Throwing one so that a global handler can turn it into a status code is control flow by exception, and it is slow and hard to follow.

## Requirements

**Functional**

1. **Middleware that catches unhandled exceptions.** It wraps the rest of the pipeline, logs the failure once, and writes the response.

2. **Use the framework's `ProblemDetails` type.** Do not invent your own error shape. Read RFC 9457 and know what each member means before you fill it in. The response's content type must be `application/problem+json`, not `application/json`.

3. **A trace id in both places.** Every error response carries an identifier, and the same identifier appears in the log line you wrote for that failure. A user reports "I got error `0HN7A...`", and you find it. This is the single feature that makes production support possible. Decide where the id comes from — there is more than one source, and they are not equally good.

4. **A cancelled request is not an error.** When a client disconnects, the pipeline throws `OperationCanceledException`. If you treat it as a server failure, your error logs fill with noise and your alerting cries wolf. Handle it separately, and decide what — if anything — to send back to a client who is no longer listening.

   You added `CancellationToken` to `IEmailSender` in Task 1.3. This is the other end of that decision.

5. **Nothing internal reaches the client.** Not the stack trace, not the exception message, not the exception type name. Exception messages routinely contain file paths, SQL fragments, and connection strings. Assume every message is unsafe.

6. **Guard against a started response.** If the response has already begun being sent, you cannot change the status code or write a body. There is a property that tells you. Decide what to do when it is true.

7. **Fix the ordering problem you found.** Your request logger currently records **200** for a crashed request. Decide where the exception handler goes relative to it, make the log tell the truth, and be ready to say what your ordering gives up.

8. **Prove it works.** Add a route that throws on purpose. Then paste, as text:
   - the JSON body the client received
   - the log line your logger produced, showing the real status code and the trace id

   Delete the route before you merge, or keep it behind a Development-only check and say why.

**The judgment deliverable (graded)**

9. **ADR 0002** at `docs/adr/0002-error-response-contract.md`, same four headings as ADR 0001.

   The decision to record: **what does an error response contain?**

   Address, in your own words:
   - What the client sees in Production, field by field, and why each field is safe.
   - Whether Development shows more. If it does, that is a branch on environment — what is the risk of that branch, and what would make it fire in the wrong place?
   - How a user reports an error you can actually find.
   - Your trigger: what would make you change this contract?

**Non-functional**
- Conventional Commits. Set the PR title before merging — you did it correctly in 2.1, keep it.
- Build green locally and in CI.
- Merged via PR.

## Traps
- **`throw ex;` instead of `throw;`** — one of them destroys the original stack trace. Know which, and why.
- **Logging the same exception twice.** Once by you, once by something else. Decide who owns the log line.
- **Setting the status code after writing the body.** Headers go out first. By then the status is fixed.
- **Returning `exception.Message` as `detail`.** It reads as helpful. It is a leak.
- **Catching in your logging middleware from 2.1.** It was told not to, for this reason — if it swallows, this handler never sees anything.
- **Handling `OperationCanceledException` as a 500.** Very common, and it poisons your error rate metric.
- **Building the JSON by hand.** The framework serialises `ProblemDetails` for you and gets the content type right.

## Things to Research
- **RFC 9457** — it obsoletes RFC 7807. Which members are required, which are optional, and what `type` is actually for
- `ProblemDetails`, `AddProblemDetails()`, and `IProblemDetailsService`
- **`IExceptionHandler`** (.NET 8+) and `UseExceptionHandler` — the framework's own way of doing this task. Read it before you decide your ordering.
- `HttpContext.TraceIdentifier` versus `Activity.Current?.Id` and W3C `traceparent` — which one survives across services, and why that matters later
- `HttpContext.Response.HasStarted`
- `throw` versus `throw ex` versus `ExceptionDispatchInfo`
- The Developer Exception Page — what it shows, and why it is Development-only

## Common Mistakes
- Writing ADR 0002 as a description of RFC 9457 instead of a decision about this API
- One `catch (Exception)` that treats every failure identically, including cancellation
- A trace id in the response that appears nowhere in the logs
- Testing only the happy path and never actually throwing

## Acceptance Criteria
- [ ] Middleware catches unhandled exceptions and writes RFC 9457 `ProblemDetails`
- [ ] Content type is `application/problem+json`
- [ ] The same trace id appears in the response body and in the log line
- [ ] `OperationCanceledException` handled separately and not logged as a server error
- [ ] No stack trace, exception message, or type name reaches a Production client
- [ ] `Response.HasStarted` handled
- [ ] Ordering decided; the request logger now records the real status for a failed request
- [ ] Proof pasted as text: the JSON response and the matching log line
- [ ] `docs/adr/0002-error-response-contract.md` written, with an observable trigger
- [ ] Build green locally and in CI
- [ ] Merged via PR, Conventional Commits, PR title set before merge

---
*Say **review** when the PR is merged.*
