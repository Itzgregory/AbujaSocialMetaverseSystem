# Toolchain Readiness: Abuja Social Metaverse

**Doc ID:** `infra-toolchain-v1.0`
**Status:** Active
**Author:** Opara Gregory
**Engineering Owner:** Opara Gregory
**Created:** 2026-05-10
**Last updated:** 2026-05-10

---

## 1. Executive Summary (TL;DR)

To run the **Abuja Social Metaverse** backend successfully, you need **Ubuntu 24.04 LTS** (recommended), **.NET 10**, **PostgreSQL 18** with **PostGIS**, **Redis 7.4+**, and the **dotnet-ef** tool.
Windows users can use WSL2 with Ubuntu 24.04, as this is the primary development and production target.

---

## 2. Target Platform & Versions

| Component | Minimum Version | Recommend Version | Required For |
| :--- | :--- | :--- | :--- |
| **Operating System** | Ubuntu 22.04 LTS | **Ubuntu 24.04 LTS** | Production & Development |
| **.NET SDK** | .NET 10.0 | .NET 10.0.100+ | Backend API |
| **PostgreSQL** | 16.0 | **PostgreSQL 18** | Primary Database |
| **PostGIS** | 3.4 | 3.5+ (Bundled with PG 18) | Geospatial queries |
| **Redis** | 6.0 | 7.4+ | Caching & SignalR Backplane |
| **dotnet-ef** | 9.0 | 10.0 | Database Migrations |

---

## 3. Repository Structure & First-Time Setup

1.  **Clone the Repository:**
    ```bash
    git clone <repository-url>
    cd AbujaSocialMetaverseSystem
    ```

2.  **Copy Environment Variables:**
    ```bash
    cp .env.example backend/.env
    ```
    *Edit `backend/.env` with your specific credentials (See Section 4).*

3.  **Install Backend Dependencies:**
    ```bash
    cd backend
    dotnet restore
    ```

---

## 4. Environment Configuration (`backend/.env`)

Create the exact keys below. **Do not use quotes** around values unless they contain spaces.

```bash
# Database (PostgreSQL)
DB_HOST=localhost
DB_PORT=5432
DB_NAME=abuja_social_metaverse_dev
DB_USER=postgres
DB_PASSWORD=your_db_password

# Connection String (Use this format for migrations)
DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=abuja_social_metaverse_dev;Username=postgres;Password=your_db_password

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379
REDIS_PASSWORD=

# JWT (Generate a long random string)
JWT_SECRET_KEY=your-super-secret-key-minimum-32-characters
JWT_ISSUER=AbujaSocialMetaverse
JWT_AUDIENCE=AbujaSocialMetaverseUsers
```

### ⚠️ Critical Checklist
- [ ] **No Quotes:** `DB_PASSWORD=MyPass123` (Correct) vs `DB_PASSWORD="MyPass123"` (Incorrect for shell sourcing).
- [ ] **No Trailing Spaces:** Ensure no space after `=` or before the value.
- [ ] **No Comments on Value Lines:** Comments must be on their own line, not at the end of a variable line.

---

## 5. Database & Infrastructure Setup

### 5.1 Ubuntu 24.04 (Primary & WSL2)

**1. Install PostgreSQL 18 & PostGIS:**
```bash
# Add PostgreSQL Official Apt Repo
sudo apt install -y postgresql-common
sudo /usr/share/postgresql-common/pgdg/apt.postgresql.org.sh -y

# Install PostgreSQL 18 and PostGIS 3.5
sudo apt update
sudo apt install -y postgresql-18 postgresql-18-postgis-3

# Start and Enable
sudo systemctl enable postgresql
sudo systemctl start postgresql

# Set Password (Replace 'postgres' with your actual user)
sudo -u postgres psql -c "ALTER USER postgres WITH PASSWORD 'your_db_password';"
```

**2. Install Redis:**
```bash
sudo apt install -y redis-server
sudo systemctl enable redis-server
sudo systemctl start redis-server
```

**3. Install .NET 10:**
```bash
# Add Microsoft Apt Repo
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install SDK
sudo apt update
sudo apt install -y dotnet-sdk-10.0
```

**4. Install EF Core Tools:**
```bash
dotnet tool install --global dotnet-ef
export PATH="$PATH:$HOME/.dotnet/tools"
echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.bashrc
```

### 5.2 macOS (Apple Silicon & Intel)

**1. Install PostgreSQL 18 & PostGIS:**
```bash
brew update
brew install postgresql@18
brew install postgis

# Start Service
brew services start postgresql@18

# Create Database (if needed) & Set Password
createdb abuja_social_metaverse_dev
psql -d postgres -c "ALTER USER $USER WITH PASSWORD 'your_db_password';" # Replace $USER with 'postgres' if using that user
```

**2. Install Redis:**
```bash
brew install redis
brew services start redis
```

**3. Install .NET 10:**
```bash
brew install dotnet@10.0
```

**4. Install EF Core Tools:**
```bash
dotnet tool install --global dotnet-ef
```

### 5.3 Windows (WSL2 - Native workflow)

**Installation:** Follow the **Ubuntu 24.04** instructions above **within your WSL2 instance**.

**Critical Path Configuration:**
```bash
# Ensure your project is stored inside the WSL filesystem (e.g., /home/user/desktop/), NOT /mnt/c/
cd /home/user/desktop/AbujaSocialMetaverseSystem
```

---

## 6. Database Preparation & Migration

### 6.1 Create the Database & Enable PostGIS

**⚠️ Required before running migrations:**

```bash
# Ubuntu / WSL2
sudo -u postgres psql -c "CREATE DATABASE abuja_social_metaverse_dev;"
sudo -u postgres psql -d abuja_social_metaverse_dev -c "CREATE EXTENSION postgis;"

# macOS
createdb abuja_social_metaverse_dev
psql -d abuja_social_metaverse_dev -c "CREATE EXTENSION postgis;"
```

### 6.2 Run Migrations

```bash
cd backend
make migrate-add name=InitialCreate
make migrate-update
```

---

## 7. Troubleshooting Common Setup Errors

Based on **actual** implementation issues:

| Error | Root Cause | Fix |
| :--- | :--- | :--- |
| `../.env: No such file or directory` | Makefile looking in wrong directory. | Move `.env` to the `backend/` folder or update Makefile to `source .env`. |
| `extension "postgis" is not available` | PostGIS library not installed in OS. | Run `brew install postgis` (macOS) or `sudo apt install postgresql-18-postgis-3` (Ubuntu). |
| `Value cannot be null. (Parameter 'logger')` | `DesignTimeDbContextFactory` missing logger. | Inject `NullLogger<ApplicationDbContext>.Instance`. |
| `Index was outside bounds of array` | Corrupt or invalid Npgsql connection string. | Re-write connection string manually. Use `Server=` instead of `Host=` as a test. |
| `EMAIL_FROM_NAME=Abuja Social: command not found` | Spaces in `.env` values without quotes. | Enclose values with spaces in quotes: `EMAIL_FROM_NAME="Abuja Social Metaverse"`. |
| `Failed to bind to http://localhost:5000` | Port already in use | `kill -9 $(lsof -t -i:5000)` |

---

## 8. Verification Checklist

Run these tests to confirm the toolchain is ready:

**PostgreSQL & PostGIS:**
```bash
psql -d abuja_social_metaverse_dev -c "SELECT PostGIS_version();"
```
- *Expected:* `3.5 ...` or similar (Not "ERROR").

**Redis:**
```bash
redis-cli ping
```
- *Expected:* `PONG`

**.NET & EF Core:**
```bash
dotnet --version
dotnet ef
```
- *Expected:* `10.0.100`, `Entity Framework Core .NET Command-line Tools 10.0.0`.

**Database Connection:**
```bash
export DB_CONNECTION_STRING="Host=localhost;Port=5432;Database=abuja_social_metaverse_dev;Username=postgres;Password=your_db_password"
cd src/Infrastructure && dotnet ef database update
```
- *Expected:* `Applying migration 'InitialCreate'... Done.`

---

## 9. Developer Workflow Commands

From the `backend/` directory:

| Command | Action |
| :--- | :--- |
| `make run` | Starts the API server. |
| `make build` | Compiles the code. |
| `make test` | Runs unit tests. |
| `make migrate-add name=<name>` | Creates a new migration. |
| `make migrate-update` | Applies migrations to the DB. |
| `dotnet run --project src/AbujaSocialMetaverse.API` | Manual run. |

---

## 10. Next Steps

Once the toolchain passes verification:

1.  **Database Schema:** Verify tables in `psql`: `\dt`.
2.  **API Health Check:** `curl https://localhost:7000/health` -> `Healthy`.
3.  **Proceed to AuthController Implementation:** Expose login/register endpoints.

---

## Change History

*v1.0 – 2026-05-10 – Initial creation based on Ubuntu 24.04 + PostgreSQL 18 + PostGIS setup and troubleshooting (Opara Gregory)*