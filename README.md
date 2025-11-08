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

---

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