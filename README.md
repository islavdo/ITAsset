# ITAssetAccounting - система учета IT-оборудования

IT Asset Accounting System. A full-featured enterprise IT asset management system developed as a coursework project. The system provides inventory management, equipment assignment, maintenance tracking, file storage, reporting, and real-time notifications for IT departments and organizations.

## Состав решения

```text
ITAssetAccounting.sln
├── src/BuildingBlocks
│   ├── ITAssetAccounting.Shared
│   └── ITAssetAccounting.Infrastructure
├── src/Services
│   ├── IdentityService
│   ├── EquipmentService
│   ├── MaintenanceService
│   ├── FileService
│   ├── ReportService
│   └── Gateway
├── src/Clients
│   ├── WebClient
│   └── DesktopClient
├── tests
│   ├── UnitTests
│   ├── IntegrationTests
│   └── Performance
├── deploy
│   ├── docker-compose.yml
│   └── nginx/nginx.conf
