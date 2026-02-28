# Employee Leave Management System

A web-based Employee Leave Management System built using ASP.NET Core 8 MVC, Entity Framework Core, and SQL Server / LocalDB.

---

## 🚀 How to Setup and Run

### 1️⃣ Install Requirements

- Download and install .NET 8.0 SDK  
  https://dotnet.microsoft.com/download/dotnet/8.0

- Install SQL Server or SQL Server LocalDB

---

### 2️⃣ Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/EmployeeLeaveManagement.git
cd EmployeeLeaveManagement
```

---

### 3️⃣ Update Database Connection

Open:

EmployeeLeaveManagement/appsettings.json

Check or update the connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EmployeeLeaveManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

If using full SQL Server, change the `Server` value accordingly.

---

### 4️⃣ Install Entity Framework Tools (If Needed)

```bash
dotnet tool install --global dotnet-ef
```

---

### 5️⃣ Restore Packages

```bash
dotnet restore
```

---

### 6️⃣ Create and Update Database

```bash
dotnet ef database update
```

This will create the database and apply migrations.

---

### 7️⃣ Run the Application

```bash
dotnet run
```

---

### 8️⃣ Open in Browser

After running the project, open:

https://localhost:5001  
or  
http://localhost:5000  

---

## 🔐 Login Credentials

| Role     | Email                 | Password  |
|----------|----------------------|-----------|
| Admin    | admin@example.com    | admin123  |
| Employee | employee@example.com | emp123    |

---

## ✨ Features

### Admin
- View dashboard
- Manage employees
- Approve or reject leave requests
- View reports

### Employee
- Apply for leave
- View leave history
- Receive real-time notifications

---

## 🛠 Technology Stack

- ASP.NET Core 8.0 MVC
- Entity Framework Core
- SQL Server / LocalDB
- Bootstrap 5
- SignalR (Real-time notifications)

---
