# 💊 Medication Scheduler (WPF)

## Features

- Dashboard overview of medications and today’s schedule  
- Medication List
- Daily Schedule  
- Warnings  
- User Profile 
- Sidebar navigation with hover/active states

---

## ⚙️ Requirements

- **Windows 10/11**
- **Visual Studio 2022** (Community or higher)
- **.NET 8.0 SDK**

---

## Getting Started

#### 1) Clone the repository
```bash
git clone https://github.com/gomezc1/Medication-App.git
```

#### 2) Open the solution
- Launch **Visual Studio 2022**
- Open the file: `MedicationScheduler.sln`

#### 3) Set the startup project
- In **Solution Explorer**, right-click **MedicationScheduler**
- Select **Set as Startup Project**

#### 4) Run the app
- Press **F5** (Start Debugging) or **Ctrl+F5** (Start Without Debugging)
- The app will launch with:
  - Sidebar navigation (**Dashboard**, **Medication List**, **Daily Schedule**, **Warnings**)
  - User profile chip (**Jane Doe**, bottom left)
  - Dashboard cards and sample schedule content

---

### 🧰 Troubleshooting

If the app doesn’t start or opens a blank window:

- Confirm `App.xaml` includes:
  ```xml
  <Application x:Class="MedicationScheduler.App"
               xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
               StartupUri="MainWindow.xaml">
  </Application>
  ```
- Make sure **MedicationScheduler** is set as the **Startup Project**
- If a stale process is locking files:
  - Close any running `MedicationScheduler.exe` in **Task Manager**
  - **Build → Clean Solution**, then **Rebuild Solution**

---


