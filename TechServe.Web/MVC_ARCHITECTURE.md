# TechServe MVC Architecture

TechServe now uses ASP.NET Core MVC for its web entry point.

## MVC responsibilities

- **Model**: `TechServe.Web/Models/DashboardViewModel.cs` contains the dashboard view model. `TechServe.Web/Database.cs` contains the database access model and records.
- **View**: `TechServe.Web/Views/Home/Index.cshtml` renders the dashboard shell and references the existing CSS and JavaScript assets.
- **Controller**: `TechServe.Web/Controllers/HomeController.cs` handles the `/` and `/Home/Index` routes and supplies the view model.

## Routing

`TechServe.Web/Program.cs` maps the conventional MVC route:

```text
/{controller=Home}/{action=Index}/{id?}
```

The customer API remains available for the live customer page:

- `GET /api/customers`
- `POST /api/customers`

The database initializer continues to create and initialize `TechServeDb` before the application accepts requests.
