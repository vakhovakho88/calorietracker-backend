# CalorieTracker.API

A RESTful API backend for a calorie tracking application built with .NET Core and Entity Framework.

## Features

- **Daily Log Management**: Track daily calorie intake with CRUD operations
- **Goal Management**: Set and manage calorie goals with different time periods
- **User-based Data**: All data is associated with specific users
- **Date Range Queries**: Retrieve logs and analyze data across custom date ranges
- **SQLite Database**: Lightweight database with Entity Framework Core
- **Repository Pattern**: Clean architecture with repository and service layers

## Project Structure

```
CalorieTracker.API/
├── Controllers/          # API controllers
├── Data/                # Database context and repositories
├── DTOs/                # Data Transfer Objects
├── Models/              # Entity models
├── Services/            # Business logic services
├── Migrations/          # Entity Framework migrations
└── Properties/          # Launch settings
```

## API Endpoints

### Daily Logs
- `GET /api/dailylogs` - Get all daily logs for a user
- `GET /api/dailylogs/{id}` - Get a specific daily log
- `GET /api/dailylogs/date/{date}` - Get daily log by date
- `GET /api/dailylogs/range` - Get daily logs within date range
- `POST /api/dailylogs` - Create a new daily log
- `PUT /api/dailylogs/{id}` - Update a daily log
- `DELETE /api/dailylogs/{id}` - Delete a daily log

### Goals
- `GET /api/goals` - Get all goals for a user
- `GET /api/goals/{id}` - Get a specific goal
- `POST /api/goals` - Create a new goal
- `PUT /api/goals/{id}` - Update a goal
- `DELETE /api/goals/{id}` - Delete a goal

## Technologies Used

- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Repository Pattern
- Dependency Injection

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository
```bash
git clone <your-repo-url>
cd calorietracker-backend
```

2. Restore dependencies
```bash
dotnet restore
```

3. Update the database
```bash
dotnet ef database update
```

4. Run the application
```bash
dotnet run
```

The API will be available at `https://localhost:7046` (HTTPS) or `http://localhost:5046` (HTTP).

## Database

The application uses SQLite with Entity Framework Core. The database file (`calorietracker.db`) is created automatically when you run the application for the first time.

### Models

- **DailyLog**: Represents a daily calorie log entry
- **Goal**: Represents calorie goals with different time periods

## Development

### Running in Development Mode
```bash
dotnet run --environment Development
```

### Building for Production
```bash
dotnet publish -c Release
```

## Testing

Test cases and requirements are documented in:
- `Testcases1.md` - Detailed test scenarios
- `Backend_anforderungen1.md` - Backend requirements

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if necessary
5. Submit a pull request

## License

This project is for educational purposes.
