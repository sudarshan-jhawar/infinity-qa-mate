# infinity-qa-mate

## Overview
The Defect Management System is designed to streamline defect logging, tracking, and prioritization using AI-powered automation. It integrates **React frontend**, **.NET backend**, and **OpenAI GPT-5** for intelligent defect analysis, leveraging historical data stored in **Azure SQL Database**.

---

## Requirements
### Functional
- Log defects via a user-friendly web interface.
- Store defect details in a secure database.
- Detect duplicate defects automatically.
- Suggest defect **priority** and **severity** based on historical patterns.
- Refine defect descriptions for clarity and consistency.

### Non-Functional
- High availability and scalability.
- Secure API communication with OpenAI GPT-5.
- Role-based access control for defect management.
- Integration with existing CI/CD pipelines.

---

## Solution Architecture
The system consists of the following components:

- **Frontend**:  
  - Built using **React** for defect logging and visualization.
- **Backend**:  
  - Developed in **.NET** to handle business logic and API integration.
- **AI Integration (OpenAI GPT-5)**:  
  - Detects duplicate defects.
  - Suggests priority and severity.
  - Refines defect descriptions.
- **Database**:  
  - **Azure SQL Database** stores historical defect data for AI analysis.
- **Configuration**:  
  - Secure storage of **OpenAI API Key** for GPT-5 integration.

---

## Architecture Diagram
<img width="748" height="499" alt="image" src="https://github.com/user-attachments/assets/1a661349-4407-4433-9463-29a643bf9f90" />

---

## ER Diagram

<img width="696" height="511" alt="image" src="https://github.com/user-attachments/assets/39df0ee2-b521-4616-a6f0-974a2522126f" />


## Key Features
- **AI-driven defect analysis** for better prioritization.
- **Duplicate detection** to reduce redundancy.
- **Historical data utilization** for accurate severity prediction.
- **Seamless integration** with enterprise systems.

---

## Setup Instructions
1. **Clone the repository**:
   ```bash
   git clone <repo-url>
   ```
2. **Install frontend dependencies**:
   ```bash
   cd frontend
   npm install
   ```
3. **Configure backend**:
   - Add **OpenAI API Key** in `appsettings.json`.
   - Set up **Azure SQL Database** connection string.
4. **Run the application**:
   ```bash
   dotnet run
   ```

---
