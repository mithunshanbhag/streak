---
name: test-automation
description: Guidelines and best practices for authoring automated tests (including unit tests and end-to-end tests).
---

### Unit Tests

- For .NET source projects, you should ideally author unit tests using XUnit, Moq, FluentAssertions and Bogus.
  - For FluentAssertion, please use the latest, stable `7.2.x` version. Do not attempt to use the `8.x` or later versions.
  - Some references for writing good unit tests in .NET:
    - [Unit testing best practices for .NET](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

### End-to-End Tests

- Please author all end-to-end tests using Playwright (with .NET SDK) and XUnit (using the `Microsoft.Playwright.XUnit` nuget package).
- For details on getting started with Playwright using XUnit: [Playwright .NET SDK](https://playwright.dev/dotnet/docs/intro).
- Playwright best practices are [documented here](https://playwright.dev/docs/best-practices).
- For consistency, please use the same testing libraries and frameworks as mentioned in the Unit Tests section.
  - But, if possible, use Playwright's own assertion library for end-to-end tests over FluentAssertions.
- It is preferable to run Playwright in Headless mode, especially since these tests will be running in CI/CD pipelines too.
