# 📋 Task 2.1 — Request-logging middleware

**Status:** 🔄 In progress
**Branch:** `feat/request-logging` off **`main`**
**Pattern:** 🧩 Chain of Responsibility

## Goal
Write middleware that logs every HTTP request — method, path, status code, and how long it took. Write it inline first so you feel the pipeline, then refactor it into a class. Along the way, prove to yourself what `next()` does, what ordering changes, and what happens when a middleware refuses to call the next one.

## Why this exists / when NOT to use it
- **Why:** the ASP.NET Core pipeline is the single most important thing to understand about the framework. Everything downstream — authentication, exception handling, CORS, rate limiting — is middleware, and they all behave the way this one behaves. Build one by hand and the rest stop being magic. It is also the first place your `ILogger` habits are set, and bad logging habits are expensive to unlearn.
- **When NOT to:** ASP.NET Core **already ships request logging** (`UseHttpLogging`, and `W3CLogger`). Writing your own is only justified if you can say what the built-in one does not give you. That justification is requirement 9, and it is what I am grading.

## Requirements

**Functional**

1. **Inline first.** Use the delegate form of middleware directly in `Program.cs`. Log the request method, the path, the response status code, and the elapsed time in milliseconds. Commit this version — it is part of the history, not a draft to be thrown away.

2. **Then refactor to a class**, plus one extension method so `Program.cs` reads as a single line. Separate commit. Be ready to say what the class bought you that the inline version did not.

3. **Structured logging, not string building.** Your log call must use a message template with **named placeholders**, and pass the values as arguments. Do not build the message with string interpolation or concatenation. You must be able to explain what is lost when you interpolate — this is the most common logging mistake in .NET, and it stays invisible until the day you need to search your logs.

4. **Choose the log level deliberately.** Every request logged at `Information` sounds reasonable until a health check runs every five seconds. Decide, and say why.

5. **Ordering experiment.** Move your middleware to at least two different positions in the pipeline and record what changes. At minimum, try it before and after `UseHttpsRedirection`. Paste real log output — not screenshots.

6. **Short-circuit experiment.** Make a version that does *not* call the next middleware. Record what the client receives and what the rest of the pipeline does. Then put it back. You are proving to yourself what `next()` actually is.

7. **Name the pattern, and say where it breaks.** 🧩 The pipeline is Chain of Responsibility. Point at the line in *your* code that is the "pass it on" step. Then answer honestly: in the classic pattern a handler either handles the request or passes it along, and control does not come back. In ASP.NET Core middleware, control **does** come back after `next()` returns. Say what that difference means, and whether "Chain of Responsibility" is still the right name for it.

8. **Log nothing secret.** Name at least two things you deliberately did not log, and why.

**The judgment deliverable (graded — this replaces one interview question)**

9. Write your **first ADR** (Architecture Decision Record) at `docs/adr/0001-hand-rolled-request-logging.md`.

   An ADR is short — one page, four headings, nothing more:

   - **Context** — what situation forced a decision
   - **Decision** — what you chose, in one sentence, in the present tense
   - **Consequences** — what you gained *and* what you gave up. Both halves. An ADR with only good news is marketing.
   - **Revisit when** — the observable event that would make you change your mind

   The decision to record: **why hand-roll this when `UseHttpLogging` exists?**

   Read what `UseHttpLogging` actually does before you write. Then answer: what does it give you for free, what does it not give you, and is the difference worth a class you now own and maintain? "I wrote it to learn the pipeline" is an honest answer and it is allowed — but then say so plainly, and say what would make you delete your version and switch.

**Non-functional**
- Conventional Commits, and **the PR title is the commit message** — set it before merging. This has now failed three times.
- Build green locally and in CI.
- Merged via PR.

## Traps
- **String interpolation in the log message.** It compiles, it looks identical in the console, and it destroys the thing structured logging exists for. Find out what you lose before you decide.
- **Reading the request body.** The body is a forward-only stream. Read it in middleware and the endpoint gets nothing. There is a way around that, and it is not free — know it exists, do not use it here.
- **Writing to the response after `next()` returns.** By then the response may already be on the wire. There is a property on the response that tells you whether that has happened.
- **`DateTime.Now` for timing.** It is not built for measuring elapsed time. Two separate reasons — find both.
- **Where the class lives.** This is an ASP.NET Core concern. Put it in the wrong project and you have undone Phase 1.
- **Catching exceptions in here.** Tempting, and wrong — that is Task 2.2's job. Middleware that does two things is two middlewares.

## Things to Research
- `app.Use` vs `app.Run` vs `app.Map` — which of them ends the pipeline, and why that matters
- **Convention-based middleware vs `IMiddleware`** — and specifically the **lifetime** difference. A convention-based middleware class is constructed once, for the life of the application. So what happens if you inject a scoped service into its constructor? You answered exactly this question in Task 1.3's interview; this is the same bug wearing a different hat.
- `ILogger` message templates and structured logging — read why the template is treated as an identity, not just as text
- `HttpContext.Response.HasStarted`
- `Stopwatch`, and `Stopwatch.GetTimestamp` for the allocation-free version
- `ILogger.IsEnabled` — when checking it first is worth the line
- `UseHttpLogging` and `W3CLogger` — required reading for requirement 9

## Common Mistakes
- Writing requirement 9 as a feature list of `UseHttpLogging` instead of a decision about this repo — third task running that the brief has had to say this
- Logging every request at `Information` and discovering the cost in production
- Middleware that quietly swallows an exception, so 2.2's global handler never sees it
- Registering the middleware in the wrong place in `Program.cs` and never noticing, because with two middlewares everything works either way

## Acceptance Criteria
- [ ] Inline version committed, then class-based version committed separately
- [ ] `Program.cs` calls one extension method
- [ ] Log call uses a message template with named placeholders — no interpolation
- [ ] Log level chosen, with a stated reason
- [ ] Ordering experiment: two positions, real log output pasted, difference explained
- [ ] Short-circuit experiment: what the client got, what the pipeline did
- [ ] Chain of Responsibility named, located in your code, and the difference from the classic pattern stated
- [ ] Two things deliberately not logged, with reasons
- [ ] `docs/adr/0001-hand-rolled-request-logging.md` — four headings, both halves of Consequences, an observable "Revisit when"
- [ ] Build green locally and in CI
- [ ] Merged via PR, Conventional Commits, **PR title corrected before merge**

---
*Say **review** when the PR is merged.*
