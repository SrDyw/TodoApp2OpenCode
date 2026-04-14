# Kanvax - Kanban Task Board
FlowBoard is a modern Kanban board application built with Blazor and MudBlazor. It allows users to manage their tasks in a visual column format with drag and drop support.
## Features
### Authentication
- User registration with username and password
- Login with validation
- Session persistence in localStorage
### Boards
- Create multiple boards with name and description
- Share boards with other users (participants)
- Dedicated URLs per board (`/board/{boardId}`)
- Dashboard with user's board list
### Kanban
- Customizable columns (name, color)
- Tasks with:
  - Title and description
  - Priority (Normal, High, Low)
  - Complete/mark tasks
  - Assign users
- Drag & drop to move tasks between columns
- Drag & drop to reorder columns
### Participant System
- Board owner has full control
- Participants can view and edit the board
- Only the owner can add/remove participants
- User list in dropdown menu
### Activity Logs
- Record of all operations
- Table view with filters (CRUD)
- On-demand loading
### UI/UX
- Modern Windows 11-style design
- Optimistic updates (no skeletons after initial load)
- Sync status bar
- MudBlazor components
## Technologies
- **Frontend**: Blazor Server (.NET 9)
- **UI Framework**: MudBlazor
- **Persistence**: localStorage
- **Styles**: CSS with MudBlazor variables
## Project Structure
```
TodoApp2OpenCode/
├── Components/
│   ├── Pages/          # Razor pages
│   │   ├── Home.razor       # Landing page
│   │   ├── Dashboard.razor  # Boards dashboard
│   │   ├── Todo.razor       # Main kanban view
│   │   └── BoardNotFound.razor
│   ├── Dialogs/        # Dialog components
│   ├── Layout/         # Layouts
│   └── Routes.razor    # Route configuration
├── Services/           # Services
│   ├── AuthService.cs      # Authentication
│   ├── BoardService.cs     # Board management
│   ├── TodoService.cs      # Task/column management
│   ├── LogService.cs       # Activity logs
│   └── BottomNavService.cs # Bottom navigation
├── Models/             # Data models
└── Program.cs          # Entry point
```
## How to Run
1. Restore dependencies:
   ```bash
   dotnet restore
   ```
2. Build:
   ```bash
   dotnet build
   ```
3. Run:
   ```bash
   dotnet run
   ```
4. Open browser at: `http://localhost:5000`
## Usage
### Create a Board
1. Log in or register
2. On the dashboard, click "New Board"
3. Enter name and description (optional)
4. The board is created and redirects automatically
### Add Tasks
1. Be on a board
2. Click "New Task"
3. Fill in title, description, priority and column
4. Save
### Manage Columns
1. Click the 3-dot menu on a column
2. Edit name/color or delete
3. Drag the header to reorder
### Share a Board
1. Click the "Participants" menu
2. Click "Add participant"
3. Search for user by name
4. Confirm
### View Logs
1. Click the "View Logs" button
2. View the board's activity table
3. Click "View Board" to go back
### Manage a Schedule
1. Create events and add participants into it
2. Visualize all todo deadline 
### Nofication System
1. Receive notification of all movements of your dashboard
