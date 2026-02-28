\# Employee Leave Management System





\## How to Setup and Run



\### Step 1: Install Requirements



1\. Download and install \[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

2\. Make sure you have SQL Server or LocalDB installed



\### Step 2: Clone the Project



```bash

git clone https://github.com/YOUR\_USERNAME/EmployeeLeaveManagement.git

cd EmployeeLeaveManagement

Step 3: Update Database Connection

Open EmployeeLeaveManagement/appsettings.json and check the connection string:



JSON



"ConnectionStrings": {

&nbsp; "DefaultConnection": "Server=(localdb)\\\\mssqllocaldb;Database=EmployeeLeaveManagementDB;Trusted\_Connection=True;MultipleActiveResultSets=true"

}

Step 4: Install EF Tools (if needed)

dotnet tool install --global dotnet-ef



Step 5: Go to Project Folder

cd EmployeeLeaveManagement



Step 6: Restore Packages

dotnet restore



Step 7: Create Database

dotnet ef database update



Step 8: Run the Application

dotnet run



Step 9: Open in Browser

Go to: https://localhost:5001 or http://localhost:5000



Login Credentials

Role	Email	Password

Admin	admin@example.com	admin123

Employee	employee@example.com	emp123

Features

Admin can:



View dashboard

Manage employees

Approve/reject leave requests

View reports

Employee can:



Apply for leave

View leave history

Get real-time notifications

Technology Stack

ASP.NET Core 8.0 MVC

Entity Framework Core

SQL Server / LocalDB

Bootstrap 5

SignalR (for notifications)

