# Tasks for Feature 003‑user-profile-db

## Phase 1 – Setup
- [X] T001 Create directory structure src/UserProfile.Api, src/UserProfile.Domain, src/UserProfile.Infrastructure
- [X] T002 Add project references in solution file

## Phase 2 – Foundations
- [X] T003 Create Entity Framework Core DbContext for UserProfiles
- [X] T004 Configure SQLite connection string in appsettings.json
- [X] T005 Scaffold migrations and apply to database

## Phase 3 – User Story US1 (Create & Edit Profile)
- [X] T006 [US1] Define UserProfile model with fields: Id, FirstName, LastName, Email, BirthDate, PhotoUrl
- [X] T007 [P] [US1] Add data annotations for required email and URL validation
- [X] T008 [US1] Implement IUserProfileRepository interface
- [X] T009 [US1] Implement UserProfileService with Create, GetById, Update methods
- [X] T010 [US1] Create ProfilesController with POST /profiles, GET /profiles/{id}, PUT /profiles/{id}
- [X] T011 [P] [US1] Write unit tests for UserProfileService
- [X] T012 [P] [US1] Write integration tests for ProfilesController using TestServer

## Final Phase – Polish
- [X] T013 Add Swagger documentation for profile endpoints
- [X] T014 Update README with usage instructions
