# 1. Hand-rolled request logging

**Date:** 2026-08-29
**Status:** Accepted

## Context

Every request should produce a log entry carrying its method, path, status code and duration. ASP.NET Core already ships this. `UseHttpLogging` writes one entry per request, redacts headers unless they are explicitly allowed, and can capture bodies with a size limit and correct stream handling — all from a single line in `Program.cs`.

The question arose because the logging this application needs is not uniform. `/health` is not an ordinary request. It runs every few seconds, so a success is noise, while a failure means a dependency such as the database or the cache is down and deserves a severity no other request would ever use. `UseHttpLogging` logs at one fixed level and cannot express that difference. The same pressure appears further out: a correlation id and a tenant id belong inside every entry, and neither is something the built-in middleware knows about.

This is also Phase 2 of a program whose method is built-in first, library second, and whose first goal for this task was to stop treating the pipeline as magic.

## Decision

We log HTTP requests with our own middleware instead of `UseHttpLogging`.

## Consequences

**Gained.** The log call is ours, so the level can follow the request rather than the framework: a failing `/health` check is logged as critical, a passing one is kept quiet, and Swagger traffic is skipped entirely. Fields we know we will need — correlation id, tenant id — can go inside the same entry rather than being emitted beside it. The pipeline also stopped being magic: `next()`, ordering, and short-circuiting were each proved by experiment. Moving the middleware to either side of `UseHttpsRedirection` showed the difference directly — placed before it, a plain HTTP request is logged twice, once as a 307 and once as the 200 the browser follows it with; placed after, that first request never reaches the middleware at all.

**Given up.** `UseHttpLogging` is one line. Ours is a class we now own, and every capability it has for free is a capability we would have to write ourselves. Two of those are not merely work, they are risk. Header redaction is on by default there and absent here — nothing in our middleware would stop a future line from writing an `Authorization` header into the log. Body logging is handled there with buffering and a size limit; done by hand it silently breaks the endpoint, because the request body is a forward-only stream and reading it consumes it. We are safe from both today only because we log neither.

## Revisit when

We need to log headers or request bodies. That is the point where the built-in middleware stops being a convenience and becomes a safety feature, and matching it by hand means writing redaction and stream buffering ourselves — two problems that are easy to get wrong and expensive to get wrong.

More generally: if this class ever stops doing anything `UseHttpLogging` could not do, it should be deleted rather than kept.
