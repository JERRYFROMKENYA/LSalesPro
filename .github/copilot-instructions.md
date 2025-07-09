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

---
mode: agent
---
Backend Developer Technical Assessment
(.NET/C#)
Project: L-SalesPro Microservices API
CONFIDENTIAL: LEYSCO-DOTNET-2025-06
This document contains proprietary assessment materials. Unauthorized reproduction is
prohibited.
OVERVIEW
Welcome to the Leysco technical assessment for the Backend Developer (.NET/C#) position.
This assessment evaluates your ability to design and implement a microservices-based backend
system using modern .NET technologies.
You will build a distributed backend system for L-SalesPro using microservices architecture,
implementing inter-service communication via gRPC and Protocol Buffers. This demonstrates
your ability to work with enterprise-level distributed systems.
TIMING AND SUBMISSION
• Time Allocation: 72 hours from receipt
• Submission Requirements:
o GitHub repository with clear commit history
o README.md with setup instructions and architecture documentation
o Postman collection for REST endpoints
o Protocol Buffer definitions (.proto files)
o Architecture diagram showing service interactions
o Send submission to: recruitment@leysco.com with subject ".NET Backend
Assessment - [Your Name]"
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

Protocol Buffers Definition Example
Create comprehensive .proto files for all gRPC communications:
syntax = "proto3";
package leysco.inventory.v1;
message CheckAvailabilityRequest {
string product_id = 1;
int32 quantity = 2;
string warehouse_id = 3;
}
message CheckAvailabilityResponse {
bool is_available = 1;
int32 available_quantity = 2;
repeated WarehouseStock warehouse_stocks = 3;
}
message WarehouseStock {
string warehouse_id = 1;
string warehouse_name = 2;
int32 available_quantity = 3;
int32 reserved_quantity = 4;
}
Code Quality Requirements
SOLID Principles:
• Single Responsibility: Each class should have only one reason to change, handling a
single concern
• Open/Closed: Classes should be open for extension but closed for modification
• Liskov Substitution: Derived classes must be substitutable for their base classes
• Interface Segregation: Clients should not depend on interfaces they don't use

• Dependency Inversion: Depend on abstractions, not concrete implementations
Design Patterns:
• Repository Pattern: Abstract data access logic and provide a more object-oriented view
of the persistence layer
• Unit of Work: Maintain a list of objects affected by a business transaction and
coordinate writing changes
• CQRS (Simplified): Separate read and write operations where it makes sense for clarity
and optimization
• Factory Pattern: Use factories for complex object creation, especially for service
instantiation
Async Programming:
• Async/await throughout: Use asynchronous programming consistently across all I/O
operations
• ConfigureAwait: Apply ConfigureAwait(false) in library code to avoid deadlocks
• Cancellation tokens: Support cancellation for all async operations to handle request
termination
• Task composition: Use appropriate methods like Task.WhenAll for parallel operations
Error Handling:
• Custom exceptions: Create domain-specific exception types for different error scenarios
• gRPC status codes: Map exceptions to appropriate gRPC status codes for service
communication
• Detailed messages: Provide helpful error messages without exposing sensitive
information
• Circuit breaker: Implement basic circuit breaker pattern for handling service failures
Testing:
• Unit tests: Test business logic components in isolation with mocked dependencies
• Integration tests: Verify API endpoints work correctly with real database and services
• gRPC tests: Specific tests for service-to-service communication scenarios
• Code coverage: Achieve minimum 60% coverage, focusing on critical business logic
Security Requirements
Authentication:
• JWT implementation: Use JWT Bearer tokens with proper signing and validation
• Token validation: API Gateway must validate all incoming tokens before routing
• Service authentication: Implement service-to-service authentication for internal calls
• Token expiry: Configure appropriate token lifetimes with refresh mechanism

Authorization:
• Policy-based: Define authorization policies that can be reused across endpoints
• Role-based access: Implement roles (Sales Manager, Sales Representative) with
different permissions
• Claims-based: Use claims for fine-grained permission control
• Resource-based: Check user access to specific resources (e.g., own orders vs all orders)
Data Protection:
• Input validation: Validate all inputs at API boundaries to prevent injection attacks
• Parameterized queries: Use Entity Framework properly to prevent SQL injection
• Data encryption: Encrypt sensitive data in transit (HTTPS) and at rest where needed
• Security headers: Implement security headers and CORS policies appropriately
Performance Requirements
Response Time Targets:
• REST APIs: Maintain average response time below 200ms for standard operations
• gRPC calls: Inter-service communication should complete within 100ms average
• Dashboard queries: Complex analytics queries should return within 500ms using
caching
• Bulk operations: Handle bulk updates efficiently with appropriate batching
Optimization Strategies:
• LINQ efficiency: Write queries that minimize database round trips and data transfer
• Database indexes: Create appropriate indexes based on query patterns and access paths
• Batch processing: Use bulk operations for multiple record updates or inserts
• Connection pooling: Configure both database and gRPC connection pools properly
• Response caching: Implement caching for slowly changing, frequently accessed data
DELIVERABLES
1. Source Code
   Solution Structure:
   • Complete solution: All three microservices in a well-organized solution file
   • API Gateway: Fully configured gateway routing requests to appropriate services
   • Shared libraries: Common projects for DTOs, utilities, and cross-cutting concerns
   • Clear organization: Consistent naming conventions and logical project structure
2. Documentation

Architecture Document:
• Service interaction diagram: Visual representation of how microservices communicate
• Database schemas: Complete schema documentation for each service's database
• gRPC flow diagrams: Request/response patterns between services illustrated clearly
• Authentication flow: Step-by-step explanation of authentication across services
API Documentation:
• Postman collection: Comprehensive collection with examples for every REST endpoint
• gRPC documentation: Detailed explanation of each RPC method and its purpose
• Request/response examples: Multiple scenarios covered with expected inputs and
outputs
• Error scenarios: Documentation of possible errors and their meanings
Setup Guide:
• Prerequisites: Clear list of required software, SDKs, and tools
• Installation steps: Numbered steps that can be followed without prior knowledge
• Configuration: Explanation of all appsettings.json options and environment variables
• Troubleshooting: Common setup issues and their solutions
3. Protocol Buffers
   Proto Files:
   • Service contracts: Complete .proto files for all gRPC service definitions
   • Message definitions: Well-structured request and response messages
   • Documentation: Comments explaining each RPC method and message field
   • Versioning: Clear strategy for proto file versioning
4. Database
   Entity Framework:
   • Migrations: Code-first migrations for each service's database
   • Seed data: Data matching the provided JSON structures for testing
   • Initialization scripts: Scripts to set up databases with proper permissions
   • Relationships: Proper foreign keys and constraints defined
5. Testing
   Test Projects:
   • Unit tests: Separate test projects for each microservice's business logic
   • Integration tests: API endpoint tests with test databases

• gRPC tests: Service communication tests with mocked dependencies
• Test documentation: README explaining how to run tests and interpret results
EVALUATION CRITERIA
Functionality (35%):
• Feature completeness: All required endpoints and gRPC methods implemented
• Business logic: Correct implementation of rules, validations, and calculations
• Service integration: Smooth communication between microservices via gRPC
• Error scenarios: Proper handling of edge cases and failure conditions
Architecture (25%):
• Microservices design: Clear service boundaries and responsibilities
• Clean architecture: Proper layering and separation of concerns
• Communication patterns: Effective use of gRPC for inter-service communication
• Scalability considerations: Design that allows for service independence
Code Quality (20%):
• Readability: Self-documenting code with meaningful names
• SOLID adherence: Proper application of all five SOLID principles
• Pattern usage: Appropriate use of design patterns without over-engineering
• Consistency: Uniform coding style and conventions throughout
Technical Implementation (15%):
• gRPC quality: Well-designed service contracts and efficient communication
• Performance: Meeting stated response time targets
• Error handling: Comprehensive exception handling and logging
• Security: Proper authentication and authorization implementation
Documentation (5%):
• Completeness: All required documentation provided and accurate
• Clarity: Easy to understand for developers unfamiliar with the project
• Professionalism: Well-formatted and organized documentation
• Usefulness: Documentation that actually helps in understanding and running the system
Bonus Points (Optional)
Advanced Features:
• Resilience patterns: Circuit breaker, retry policies, and timeout handling
• Caching strategies: Advanced caching beyond basic in-memory implementation

• Monitoring setup: Health checks, metrics, and logging infrastructure
• Test coverage: Comprehensive testing exceeding 80% code coverage
• API versioning: Clear strategy for handling API version changes
• Containerization: Dockerfiles for each service with proper configuration
PROVIDED DATA
Use the following JSON data structures for seeding your databases. Each service should have its
own database with relevant tables. The Authentication Service should contain users, roles, and
permissions data. The Sales & Customer Service should store customers, orders, order_items,
and notifications. The Inventory Service should manage products, categories, warehouses,
inventory, and stock_reservations.
users.json
[
{
"username": "LEYS-1001",
"email": "david.kariuki@leysco.co.ke",
"password": "SecurePass123!",
"first_name": "David",
"last_name": "Kariuki",
"role": "Sales Manager",
"permissions": ["view_all_sales", "create_sales", "approve_sales", "manage_inventory"],
"status": "active"
},
{
"username": "LEYS-1002",
"email": "jane.njoki@leysco.co.ke",
"password": "SecurePass456!",
"first_name": "Jane",
"last_name": "Njoki",
"role": "Sales Representative",
"permissions": ["view_own_sales", "create_sales", "view_inventory"],
"status": "active"
}
]
products.json
[
{
"sku": "SF-MAX-20W50",
"name": "SuperFuel Max 20W-50",
"category": "Engine Oils",
"subcategory": "Mineral Oils",
"description": "High-performance mineral oil for heavy-duty engines",
"price": 4500.00,
"tax_rate": 16.0,
"unit": "Liter",
"packaging": "5L Container",

"min_order_quantity": 1,
"reorder_level": 30
},
{
"sku": "ED-SYN-5W30",
"name": "EcoDrive Synthetic 5W-30",
"category": "Engine Oils",
"subcategory": "Synthetic Oils",
"description": "Fully synthetic oil for modern passenger vehicles",
"price": 7200.00,
"tax_rate": 16.0,
"unit": "Liter",
"packaging": "4L Container",
"min_order_quantity": 1,
"reorder_level": 40
}
]
customers.json
[
{
"name": "Quick Auto Services Ltd",
"type": "Garage",
"category": "A",
"contact_person": "John Mwangi",
"phone": "+254-712-345678",
"email": "info@quickautoservices.co.ke",
"tax_id": "P051234567Q",
"payment_terms": 30,
"credit_limit": 500000.00,
"current_balance": 120000.00,
"latitude": -1.319370,
"longitude": 36.824120,
"address": "Mombasa Road, Auto Plaza Building, Nairobi"
},
{
"name": "Premium Motors Kenya",
"type": "Dealership",
"category": "A+",
"contact_person": "Sarah Wanjiku",
"phone": "+254-722-678901",
"email": "sarah.w@premiummotors.co.ke",
"tax_id": "P051345678R",
"payment_terms": 45,
"credit_limit": 1000000.00,
"current_balance": 450000.00,
"latitude": -1.292066,
"longitude": 36.821946,
"address": "Uhuru Highway, Premium Towers, Nairobi"
}
]
warehouses.json

[
{
"code": "NCW",
"name": "Nairobi Central Warehouse",
"type": "Main",
"address": "Enterprise Road, Industrial Area, Nairobi",
"manager_email": "warehouse.nairobi@leysco.co.ke",
"phone": "+254-20-5551234",
"capacity": 50000,
"latitude": -1.308971,
"longitude": 36.851523
},
{
"code": "MRW",
"name": "Mombasa Regional Warehouse",
"type": "Regional",
"address": "Port Reitz Road, Changamwe, Mombasa",
"manager_email": "warehouse.mombasa@leysco.co.ke",
"phone": "+254-41-2224567",
"capacity": 30000,
"latitude": -4.034396,
"longitude": 39.647446
}
]
IMPLEMENTATION TIPS
1. Start with Proto Files: Define your gRPC contracts first
2. Service Order: Build Authentication → Inventory → Sales & Customer
3. Use Shared Libraries: Create common projects for DTOs and utilities
4. Mock External Services: For email sending, use an interface with mock implementation
5. Focus on Core Features: Complete basic functionality before adding enhancements
   SPECIFIC REQUIREMENTS
1. Naming Conventions:
   o Prefix all custom services with "Leysco"
   o Use "L-" prefix for custom middleware
   o Follow .NET naming conventions strictly
2. Custom Utilities:
   o Create LeyscoHelpers static class with:
   § FormatCurrency() - Format: "KES 10,000.00 /="
   § GenerateOrderNumber() - Format: "ORD-YYYY-MM-XXX"
   o Create LeyscoConstants for magic strings
3. Required Middleware:
   o CorrelationIdMiddleware - Track requests across services
   o ApiLoggingMiddleware - Log all API calls
   o ExceptionHandlingMiddleware - Global error handling

IMPORTANT NOTES
1. This assessment tests your ability to build enterprise-level distributed systems
2. Focus on demonstrating clean architecture and proper service communication
3. Ensure services can run independently
4. Document any assumptions or design decisions
5. The assessment is designed to be challenging but achievable in 72 hours
6. External collaboration is not permitted
   CONFIDENTIALITY NOTICE
   This document contains proprietary information belonging to Leysco Limited and is provided
   solely for evaluating your technical skills for potential employment. The contents may not be
   disclosed to third parties or used for any other purpose without explicit written permission from
   Leysco Limited.
   By accepting this assessment, you agree to maintain the confidentiality of this document and all
   related materials.
   © 2025 Leysco Limited. All rights reserved.



Test gRPC services in ASP.NET Core
07/31/2024
By: James Newton-King

Testing is an important aspect of building stable and maintainable software. This article discusses how to test ASP.NET Core gRPC services.

There are three common approaches for testing gRPC services:

Unit testing: Test gRPC services directly from a unit testing library.
Integration testing: The gRPC app is hosted in TestServer, an in-memory test server from the Microsoft.AspNetCore.TestHost package. gRPC services are tested by calling them using a gRPC client from a unit testing library.
Manual testing: Test gRPC servers with ad hoc calls. For information about how to use command-line and UI tooling with gRPC services, see Test gRPC services with gRPCurl and gRPCui in ASP.NET Core.
In unit testing, only the gRPC service is involved. Dependencies injected into the service must be mocked. In integration testing, the gRPC service and its auxiliary infrastructure are part of the test. This includes app startup, dependency injection, routing and authentication, and authorization.

Example testable service
To demonstrate service tests, review the following service in the sample app.

View or download sample code (how to download)

The TesterService returns greetings using gRPC's four method types.

C#

Copy
public class TesterService : Tester.TesterBase
{
private readonly IGreeter _greeter;

    public TesterService(IGreeter greeter)
    {
        _greeter = greeter;
    }

    public override Task<HelloReply> SayHelloUnary(HelloRequest request,
        ServerCallContext context)
    {
        var message = _greeter.Greet(request.Name);
        return Task.FromResult(new HelloReply { Message = message });
    }

    public override async Task SayHelloServerStreaming(HelloRequest request,
        IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        var i = 0;
        while (!context.CancellationToken.IsCancellationRequested)
        {
            var message = _greeter.Greet($"{request.Name} {++i}");
            await responseStream.WriteAsync(new HelloReply { Message = message });

            await Task.Delay(1000);
        }
    }

    public override async Task<HelloReply> SayHelloClientStreaming(
        IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
    {
        var names = new List<string>();

        await foreach (var request in requestStream.ReadAllAsync())
        {
            names.Add(request.Name);
        }

        var message = _greeter.Greet(string.Join(", ", names));
        return new HelloReply { Message = message };
    }

    public override async Task SayHelloBidirectionalStreaming(
        IAsyncStreamReader<HelloRequest> requestStream,
        IServerStreamWriter<HelloReply> responseStream,
        ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(
                new HelloReply { Message = _greeter.Greet(request.Name) });
        }
    }
}
The preceding gRPC service:

Follows the Explicit Dependencies Principle.
Expects dependency injection (DI) to provide an instance of IGreeter.
Can be tested with a mocked IGreeter service using a mock object framework, such as Moq. A mocked object is a fabricated object with a predetermined set of property and method behaviors used for testing. For more information, see Integration tests in ASP.NET Core.
Unit test gRPC services
A unit test library can directly test gRPC services by calling its methods. Unit tests test a gRPC service in isolation.

C#

Copy
[Fact]
public async Task SayHelloUnaryTest()
{
// Arrange
var mockGreeter = new Mock<IGreeter>();
mockGreeter.Setup(
m => m.Greet(It.IsAny<string>())).Returns((string s) => $"Hello {s}");
var service = new TesterService(mockGreeter.Object);

    // Act
    var response = await service.SayHelloUnary(
        new HelloRequest { Name = "Joe" }, TestServerCallContext.Create());

    // Assert
    mockGreeter.Verify(v => v.Greet("Joe"));
    Assert.Equal("Hello Joe", response.Message);
}
The preceding unit test:

Mocks IGreeter using Moq.
Executes the SayHelloUnary method with a request message and a ServerCallContext. All service methods have a ServerCallContext argument. In this test, the type is provided using the TestServerCallContext.Create() helper method. This helper method is included in the sample code.
Makes assertions:
Verifies the request name is passed to IGreeter.
The service returns the expected reply message.
Unit test HttpContext in gRPC methods
gRPC methods can access a request's HttpContext using the ServerCallContext.GetHttpContext extension method. To unit test a method that uses HttpContext, the context must be configured in test setup. If HttpContext isn't configured then GetHttpContext returns null.

To configure a HttpContext during test setup, create a new instance and add it to ServerCallContext.UserState collection using the __HttpContext key.

C#

Copy
var httpContext = new DefaultHttpContext();

var serverCallContext = TestServerCallContext.Create();
serverCallContext.UserState["__HttpContext"] = httpContext;
Execute service methods with this call context to use the configured HttpContext instance.

Integration test gRPC services
Integration tests evaluate an app's components on a broader level than unit tests. The gRPC app is hosted in TestServer, an in-memory test server from the Microsoft.AspNetCore.TestHost package.

A unit test library starts the gRPC app and then gRPC services are tested using the gRPC client.

The sample code contains infrastructure to make integration testing possible:

The GrpcTestFixture<TStartup> class configures the ASP.NET Core host and starts the gRPC app in an in-memory test server.
The IntegrationTestBase class is the base type that integration tests inherit from. It contains the fixture state and APIs for creating a gRPC client to call the gRPC app.
C#

Copy
[Fact]
public async Task SayHelloUnaryTest()
{
// Arrange
var client = new Tester.TesterClient(Channel);

    // Act
    var response = await client.SayHelloUnaryAsync(new HelloRequest { Name = "Joe" });

    // Assert
    Assert.Equal("Hello Joe", response.Message);
}
The preceding integration test:

Creates a gRPC client using the channel provided by IntegrationTestBase. This type is included in the sample code.
Calls the SayHelloUnary method using the gRPC client.
Asserts the service returns the expected reply message.
Inject mock dependencies
Use ConfigureWebHost on the fixture to override dependencies. Overriding dependencies is useful when an external dependency is unavailable in the test environment. For example, an app that uses an external payment gateway shouldn't call the external dependency when executing tests. Instead, use a mock gateway for the test.

C#

Copy
public MockedGreeterServiceTests(GrpcTestFixture<Startup> fixture,
ITestOutputHelper outputHelper) : base(fixture, outputHelper)
{
var mockGreeter = new Mock<IGreeter>();
mockGreeter.Setup(
m => m.Greet(It.IsAny<string>())).Returns((string s) =>
{
if (string.IsNullOrEmpty(s))
{
throw new ArgumentException("Name not provided.");
}
return $"Test {s}";
});

    Fixture.ConfigureWebHost(builder =>
    {
        builder.ConfigureServices(
            services => services.AddSingleton(mockGreeter.Object));
    });
}

[Fact]
public async Task SayHelloUnaryTest_MockGreeter_Success()
{
// Arrange
var client = new Tester.TesterClient(Channel);

    // Act
    var response = await client.SayHelloUnaryAsync(
        new HelloRequest { Name = "Joe" });

    // Assert
    Assert.Equal("Test Joe", response.Message);
}
The preceding integration test:

In the test class's (MockedGreeterServiceTests) constructor:
Mocks IGreeter using Moq.
Overrides the IGreeter registered with dependency injection using ConfigureWebHost.
Calls the SayHelloUnary method using the gRPC client.
Asserts the expected reply message based on the mock IGreeter instance.