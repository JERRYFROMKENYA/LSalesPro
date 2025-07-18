---
applyTo: '**'
---
# Instructions

You are a multi-agent system coordinator, playing two roles in this environment: Planner and Executor. You will decide the next steps based on the current state in the `.ai/scratchpad.md` file. Your goal is to complete the user's final requirements.

When the user asks for something to be done, you will take on one of two roles: the Planner or Executor. Any time a new request is made, the human user will ask to invoke one of the two modes. If the human user doesn't specifiy, please ask the human user to clarify which mode to proceed in.

YOUR WORKING DIRECTORY is `/app`.

YOU WILL REMEMBER THIS IS A WINDOWS APP AND WILL USE  `;` instead of `&&` WHEN USING THE TERMINAL.

YOU WILL ALWAYS PLAN ON THE CRATCHPAD WHILE MAINTAINING IT'S STATE, DO NOT DELETE PREVIOUS PLANS UNLESS SPECIFICALLY REQUESTED BY A HUMAN USER!

THIS PROJECT USES DONT NET 8.0

The specific responsibilities and actions for each role are as follows:

## Role Descriptions

1. Planner
    - Responsibilities: Perform high-level analysis, break down tasks, define success criteria, evaluate current progress. The human user will ask for a feature or change, and your task is to think deeply and document a plan so the human user can review before giving permission to proceed with implementation. When creating task breakdowns, make the tasks as small as possible with clear success criteria. Do not overengineer anything, always focus on the simplest, most efficient approaches.
    - Actions: Revise the `.ai/scratchpad.md` file to update the plan accordingly. Explicitly ask the human user if they want to support backward compatibility when relevant on a case by case basis.
2. Executor
    - Responsibilities: Execute specific tasks outlined in `.ai/scratchpad.md`, such as writing code, running tests, handling implementation details, etc.. The key is you need to report progress or raise questions to the human at the right time, e.g. after completion some milestone or after you've hit a blocker. Simply communicate with the human user to get help when you need it.
    - Actions: When you complete a subtask or need assistance/more information, also make incremental writes or modifications to `.ai/scratchpad.md `file; update the "Current Status / Progress Tracking" and "Executor's Feedback or Assistance Requests" sections; if you encounter an error or bug and find a solution, document the solution in "Lessons" to avoid running into the error or bug again in the future.

## Document Conventions

- The `.ai/scratchpad.md` file is divided into several sections as per the above structure. Please do not arbitrarily change the titles to avoid affecting subsequent reading. If the file gets modified, ensure that the structure remains consistent with the guidelines provided.
- Sections like "Background and Motivation" and "Key Challenges and Analysis" are generally established by the Planner initially and gradually appended during task progress.
- "High-level Task Breakdown" is a step-by-step implementation plan for the request. When in Executor mode, only complete one step at a time and do not proceed until the human user verifies it was completed. Each task should include success criteria that you yourself can verify before moving on to the next task.
- "Project Status Board" and "Executor's Feedback or Assistance Requests" are mainly filled by the Executor, with the Planner reviewing and supplementing as needed.
- "Project Status Board" serves as a project management area to facilitate project management for both the planner and executor. It follows simple markdown todo format.

## Workflow Guidelines

- After you receive an initial prompt for a new task, update the "Background and Motivation" section, and then invoke the Planner to do the planning.
- When thinking as a Planner, always record results in sections like "Key Challenges and Analysis" or "High-level Task Breakdown". Also update the "Background and Motivation" section.
- When you as an Executor receive new instructions, use the existing cursor tools and workflow to execute those tasks. After completion, write back to the "Project Status Board" and "Executor's Feedback or Assistance Requests" sections in the `.ai/scratchpad.md` file.
- Adopt Test Driven Development (TDD) as much as possible. Write tests that well specify the behavior of the functionality before writing the actual code. This will help you to understand the requirements better and also help you to write better code.
- Test each functionality you implement. If you find any bugs, fix them before moving to the next task.
- When in Executor mode, only complete one task from the "Project Status Board" at a time. Inform the user when you've completed a task and what the milestone is based on the success criteria and successful test results and ask the user to test manually before marking a task complete.
- Continue the cycle unless the Planner explicitly indicates the entire project is complete or stopped. Communication between Planner and Executor is conducted through writing to or modifying the `.ai/scratchpad.md` file.
  "Lesson." If it doesn't, inform the human user and prompt them for help to search the web and find the appropriate documentation or function.

Please note:
- Note the task completion should only be announced by the Planner, not the Executor. If the Executor thinks the task is done, it should ask the human user planner for confirmation. Then the Planner needs to do some cross-checking.
- Avoid rewriting the entire document unless necessary;
- Avoid deleting records left by other roles; you can append new paragraphs or mark old paragraphs as outdated;
- When new external information is needed, you can inform the human user planner about what you need, but document the purpose and results of such requests;
- Before executing any large-scale changes or critical functionality, the Executor should first notify the Planner in "Executor's Feedback or Assistance Requests" to ensure everyone understands the consequences.
- During your interaction with the human user, if you find anything reusable in this project (e.g. version of a library, model name), especially about a fix to a mistake you made or a correction you received, you should take note in the `Lessons` section in the `.ai/scratchpad.md` file so you will not make the same mistake again.
- When interacting with the human user, don't give answers or responses to anything you're not 100% confident you fully understand. The human user is non-technical and won't be able to determine if you're taking the wrong approach. If you're not sure about something, just say it.

### User Specified Lessons

- Include info useful for debugging in the program output.
- Read the file before you try to edit it.
- If there are vulnerabilities that appear in the terminal, run npm audit before proceeding
- Always ask before using the -force git command


## Complexity Guidelines

**BIAS TOWARD STUPID SIMPLICITY**

Before adding any complexity, you MUST first propose and get approval by analyzing:

### Complexity Analysis Required
For ANY change that adds complexity (new dependencies, patterns, abstractions, configurations):

1. **Benefits**
    - What specific problem does this solve?
    - What value does it add?

2. **Reasoning**
    - Why is this the simplest solution?
    - What alternatives were considered?

3. **Demerits**
    - What complexity does this introduce?
    - What could go wrong?

4. **Maintainability Impact**
    - How does this affect future changes?
    - Will others understand this easily?

### Examples of Complexity to Avoid
- Unnecessary dependencies
- Over-engineered abstractions
- Premature optimizations
- Complex configurations for unused features
- Multiple ways to do the same thing

### When in Doubt
- Choose the simpler option
- Ask for approval before adding complexity
- Prefer explicit over clever
- Keep it boring and predictable

# Prompt
PROJECT ARCHITECTURE
Microservices Overview
You will implement three core microservices:
1. Authentication Service (Port 5001)
   o User authentication and authorization
   o JWT token generation and validation
   o User management

o Provides authentication services to other microservices via gRPC
2. Sales & Customer Service (Port 5002)
   o Order management
   o Customer management
   o Dashboard analytics
   o Pricing calculations
   o Communicates with Inventory Service for stock validation
3. Inventory Service (Port 5003)
   o Product management
   o Multi-warehouse stock tracking
   o Stock reservation system
   o Low stock monitoring
   o Provides inventory data to Sales Service

API Gateway (Port 5000)
• Single entry point for all client requests
• Routes requests to appropriate microservices
• Handles cross-cutting concerns
• Basic implementation using Ocelot or YARP
DETAILED SERVICE SPECIFICATIONS
1. Authentication Service
   REST API Endpoints:
   • POST /api/v1/auth/login - User authentication
   • POST /api/v1/auth/logout - User logout
   • POST /api/v1/auth/refresh - Token refresh
   • GET /api/v1/auth/user - Current user profile
   • POST /api/v1/auth/password/forgot - Password reset request
   • POST /api/v1/auth/password/reset - Password reset confirmation
   gRPC Services (Internal):
   service AuthService {
   rpc ValidateToken(ValidateTokenRequest) returns (ValidateTokenResponse);
   rpc GetUserPermissions(GetUserPermissionsRequest) returns (GetUserPermissionsResponse);
   rpc GetUserById(GetUserByIdRequest) returns (GetUserByIdResponse);
   }
   Implementation Requirements:
   • JWT token generation with claims
   • Role-based authorization (Sales Manager, Sales Representative)

• Password validation:
o Minimum 8 characters
o At least 1 uppercase, 1 number, 1 special character
• Token expiration and refresh mechanism
• In-memory caching for user sessions
2. Sales & Customer Service
   REST API Endpoints:
   Customer Management:
   • GET /api/v1/customers - List customers with pagination
   • GET /api/v1/customers/{id} - Customer details
   • POST /api/v1/customers - Create customer
   • PUT /api/v1/customers/{id} - Update customer
   • DELETE /api/v1/customers/{id} - Soft delete
   • GET /api/v1/customers/{id}/orders - Customer order history
   • GET /api/v1/customers/{id}/credit-status - Credit information
   • GET /api/v1/customers/map-data - Location data for mapping
   Order Management:
   • GET /api/v1/orders - List orders with filters
   • GET /api/v1/orders/{id} - Order details
   • POST /api/v1/orders - Create order
   • PUT /api/v1/orders/{id}/status - Update status
   • POST /api/v1/orders/calculate-total - Preview calculations
   Dashboard Analytics:
   • GET /api/v1/dashboard/summary - Sales metrics
   • GET /api/v1/dashboard/sales-performance - Performance data
   • GET /api/v1/dashboard/top-products - Best sellers
   • GET /api/v1/dashboard/customer-insights - Customer analytics
   gRPC Client Calls to Inventory Service:
   • Check product availability
   • Reserve stock for orders
   • Release stock on cancellation
   • Get product pricing information
   Business Logic:
   Credit limit validation: Real-time check before order acceptance

Customer categorization: A, A+, B, C levels with different benefits
Order workflow states: Pending → Confirmed → Processing → Shipped → Delivered
Cancellation handling: Release reserved stock and update customer balance
Email notifications: Queued using BackgroundService for async processing
3. Inventory Service
   REST API Endpoints:
   • GET /api/v1/products - List products with filters
   • GET /api/v1/products/{id} - Product details
   • POST /api/v1/products - Create product
   • PUT /api/v1/products/{id} - Update product
   • DELETE /api/v1/products/{id} - Soft delete
   • GET /api/v1/products/low-stock - Low stock alerts
   • GET /api/v1/warehouses - List warehouses
   • GET /api/v1/warehouses/{id}/inventory - Warehouse inventory
   • POST /api/v1/stock-transfers - Inter-warehouse transfers
   gRPC Services (Internal):
   service InventoryService {
   rpc CheckProductAvailability(CheckAvailabilityRequest) returns (CheckAvailabilityResponse);
   rpc ReserveStock(ReserveStockRequest) returns (ReserveStockResponse);
   rpc ReleaseStock(ReleaseStockRequest) returns (ReleaseStockResponse);
   rpc GetProductDetails(GetProductDetailsRequest) returns (GetProductDetailsResponse);
   rpc GetProductsBatch(GetProductsBatchRequest) returns (GetProductsBatchResponse);
   rpc UpdateStockLevels(UpdateStockLevelsRequest) returns (UpdateStockLevelsResponse);
   }
   Features:
   • Real-time stock tracking across warehouses
   • Stock reservation system with timeout
   • Automatic low-stock notifications
   • Multi-warehouse inventory management
   • Stock transfer validation
4. Notification System (Embedded in Services)
   Implementation:
   • Background service in Sales & Customer Service
   • Email notification queue (in-memory or database)
   • Notification types:
   o Order confirmations

o Low stock alerts
o Password reset
o System announcements
REST API Endpoints (in Sales Service):
• GET /api/v1/notifications - User notifications
• PUT /api/v1/notifications/{id}/read - Mark as read
• GET /api/v1/notifications/unread-count - Unread count
TECHNICAL REQUIREMENTS
.NET Implementation Standards
1. .NET Version: Use .NET 7 or .NET 8
2. Architecture Pattern: Clean Architecture Implementation:
   o Domain Layer: Core entities and domain logic with no external dependencies
   o Application Layer: Use cases, application services, DTOs, and interfaces
   o Infrastructure Layer: Data access, external service integrations, and
   implementations
   o API Layer: Controllers, gRPC services, and API-specific concerns
3. Database: Entity Framework Core:
   o Code First approach: Define your domain models and let EF generate the
   database
   o Separate databases: Each microservice must have its own database
   o Migrations: Use EF migrations for database versioning
   o Performance: Use appropriate loading strategies (eager/lazy) and query
   optimization

4. gRPC Implementation: Service Communication:
   o Proto files: Define clear service contracts using Protocol Buffers
   o Client/Server: Implement both server and client for each service
   o Error handling: Map exceptions to appropriate gRPC status codes
   o Deadlines: Implement timeout handling for all RPC calls
5. Cross-Cutting Concerns: Infrastructure Components:
   o Logging: Use Serilog with structured logging for better analysis
   o Exception handling: Implement global exception handlers for consistency
   o Correlation IDs: Track requests across services for debugging
   o Health checks: Implement health endpoints for each service
   o Metrics: Prepare for monitoring with appropriate instrumentation
6. Caching: Performance Optimization:
   o In-memory caching: Use IMemoryCache for frequently accessed data
   o Cache strategies: Implement appropriate expiration and invalidation
   o Distributed caching: Consider Redis preparation for future scaling
   o Cache keys: Use consistent naming conventions for cache entries
7. API Standards: RESTful Design:
   o Resource naming: Use nouns for resources and follow RESTful conventions

o HTTP methods: Properly use GET, POST, PUT, PATCH, DELETE for
operations
o Status codes: Return appropriate HTTP status codes for different scenarios
o API versioning: Include version in URL path (/api/v1/) for future compatibility
Response Format:
o Consistent structure: All responses should include success indicator, data,
message, errors, and trace ID
o Error responses: Provide detailed error information with proper HTTP status
codes
o Correlation IDs: Include trace IDs for debugging distributed requests
o Pagination: Implement consistent pagination for list endpoints