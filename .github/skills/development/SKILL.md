---
name: development
description: Guidelines and best practices for development with .NET.
---

## UI Components / Pages

- All UI components are Razor components organized under `src\{appName}\Components\`.
- The various components are organized by feature area, with a general pattern of having sub-folders for `Pages`, `Shared`, and `Layout` components:
- The various UI components will inject services to perform any necessary business logic or data fetching, but should not contain any complex logic themselves. They should be thin and focused on rendering the UI and handling user interactions.

## Controllers

- All API controllers (and trigger-activated Azure functions) are organized under `src\{appName}\Controllers\`.
- Controllers will inject services to perform any necessary business logic or data fetching, but should not contain any complex logic themselves. They should be thin and focused on handling HTTP requests and responses, and delegating to services for the actual work.
- Controllers should only accept and return DTOs/REST models. Not view/storage/persistence models or database entities.
- To the greatest extent possible, try to use the controller base classes defined in the Nucleus nuget package. A benefit of this is that it automatically converts any validation exceptions (thrown by FluentValidation) or any custom exceptions (thrown by service layer) into appropriate ActionResult / HTTP responses with error details.

## Service Layer

- Services are centralized under: `src\{AppName}\Services\` or `src\{AppName}.Core\Services\`.
- Define three sub-folders:
  - `Interfaces` for service interfaces.
  - `Implementations` for concrete service implementations. 
    - To the greatest extent possible, try to use the service base classes defined in the Nucleus nuget package (see details below).
    - Else if necessary, create new service base classes.
    - Every public method on services that accept model(s) as arguments must validate the input before any processing occurs (see below).
    - Service methods should only accept and return DTOs/REST models and ViewModels. Not storage/persistence models or database entities.
    - Service methods can use AutoMapper to map between DTOs/ViewModels and storage models/entities, but the service method signatures should not directly reference storage models/entities.
  - `Validators` for FluentValidation validators of inputs: models, DTOs, etc.
    - Define all your FluentValidation validator classes here.
- Miscellaneous:
  - For all the app's HTTP calls to 3rd parties, we should define services that are injected via DI as typed clients or named clients.

## Repository Layer

- Repositories are centralized under: `src\{AppName}\Repositories\` or `src\{AppName}.Core\Repositories\`.
- Define two sub-folders:
  - `Interfaces` for repository interfaces.
  - `Implementations` for concrete repository implementations.
    - To the greatest extent possible, try to use the repository base classes defined in the Nucleus nuget package (see details below).
    - When using EFCore, the DBContext class should be defined in the `Implementations` folder, and repository implementations can depend on it for data access.
    - Repository methods should only accept, return and operate upon storage/persistence models or database entities. Not DTOs or ViewModels.

## Models

- Models are centralized under: `src\{AppName}\Models\` or `src\{AppName}.Core\Models\`.
- For UI: Create a `ViewModels` sub-folder:
  - Input models go into a `InputModels` sub-folder.
  - Result models go into a `ResultModels` sub-folder.
    - Result models include computed display properties (for example, currency-formatted strings) to keep display formatting out of core math logic.
- For API: Create the following sub-folders:
  - `REST` for REST API request/response models, DTOs, etc.
  - `Storage` for persistence models (for example, Cosmos DB entities).
  - `Events` for event stream models.
- Define a `MapperProfile` class for AutoMapper under `src\{AppName}\Misc\AutoMapper\` to centralize mapping configuration between various model types.

## Custom Exceptions

- Custom exception types are centralized under `src\{AppName}\Exceptions\` or `src\{AppName}.Core\Exceptions\`.
- To the greatest extent possible, the custom exceptions should inherit from base exception classes defined in the Nucleus nuget package. This allows the custom exception to define its own conversion to an ActionResult or HTTP response.

## Global Constants / Routing / Config Keys

- Constants are centralized under: `src\{AppName}\Constants\` or `src\{AppName}.Core\Constants\`.
- Existing pattern:
  - `RouteConstants` for navigation routes
  - `ApiConstants` for well-known API endpoint paths
  - `UrlConstants` for external URLs
  - `HttpClientNameConstants` for named HTTP client keys
  - `ConfigKeys` for configuration keys
- These classes will be declared as `public static {className}` with `public const string` fields.
- Any Enums can also be declared as `public enum {enumName}`.
- Centralize magic strings and enums in these classes rather than in individual components or services.

## Other Important Notes

### UI Development

- To the greatest extent possible, try to use built-in the defined themes, styles, typography, color palettes, and components from MudBlazor when implementing the UI in Blazor. This will help maintain a consistent look and feel across the app and also speed up development. Try to avoid custom CSS and styles unless absolutely necessary.

### Dependency Injection Pattern ⚠️

DI is centralized via startup extension methods:
- `src\{appName}}\Misc\ExtensionMethods\WebAssemblyHostBuilderExtensions.cs`

Registration pattern:
- AutoMapper profile registration (`MapperProfile`)
- MediatR registration via assembly scan
- MudBlazor services registration
- FluentValidation validators registered by assembly scanning
- Calculator services registered as `AddSingleton<ICalculator<...>, ...Calculator>()`
- Repositories registered as `AddTransient<...>()`

Keep new registrations in `ConfigureServices()` to preserve a single composition root.

### NuGet Packages

- Add the `MithunShanbhag.Nucleus` package (latest pre-release version). 
  - Generally, this has all the necessary base abstractions (for repositories, event streams, services, etc.), helper utilities, and shared dependencies.
  - Source code: [mithunshanbhag/Nucleus](https://github.com/mithunshanbhag/nucleus)

### Formatting and Quality Workflow ⚠️

Before completing changes, run formatting and verification:

1. `dotnet format`
   - Ensures code aligns with repository `.editorconfig` conventions.
2. `dotnet build --nologo`
