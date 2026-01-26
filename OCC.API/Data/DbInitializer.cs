using OCC.API.Data;
using OCC.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace OCC.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context, OCC.API.Services.PasswordHasher hasher, bool isDevelopment)
        {
            // Prepare DB (Apply Migrations)
            context.Database.Migrate();

            // Look for any users.
            /* 
             * Previous check: if (context.Users.Any()) return; 
             * This prevents seeding if ANY user exists (e.g. invalid test users).
             * We will now explicitly ensure the Admin user exists.
             */

            var adminEmail = "neil@mdk.co.za";
            var adminUser = context.Users.FirstOrDefault(u => u.Email == adminEmail);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    Email = adminEmail,
                    Password = hasher.HashPassword("pass"), // Hashed Password
                    FirstName = "Neil",
                    LastName = "Admin",
                    UserRole = UserRole.Admin,
                    IsApproved = true,
                    IsEmailVerified = true
                };
                context.Users.Add(adminUser);
                context.SaveChanges();
            }
            else
            {
                // Optional: Ensure password matches "pass" hash if debugging, 
                // but usually we don't overwrite passwords in prod. 
                // For this dev request: "Can we not seed me as a user created with password hash?"
                // Implicitly implies valid user should exist. 
                // If the user forgot their password (likely "pass" logic from failsafe), 
                // we could force reset it here, but that's aggressive.
                // Assuming existence is enough given the failsafe removal.
            }

            // Ensure Admin User is linked to Admin Employee (if exists)
            // This is crucial for "My Summary" filtering verification
            var adminEmployee = context.Employees.FirstOrDefault(e => e.Email == adminEmail);
            if (adminEmployee != null && adminEmployee.LinkedUserId != adminUser.Id)
            {
                adminEmployee.LinkedUserId = adminUser.Id;
                context.SaveChanges();
            }

            // Other default users if needed...
            // Other default users if needed...
            
            if (isDevelopment)
            {
               Console.WriteLine($"[Seeding] Environment is Development. Seeding checks initiating...");
               // SAFETY LOCK: Commented out to prevent accidental production seeding.
               // Uncomment these manually only if you intend to wipe/seed a DEV database.
               // SeedEmployees(context);
               // SeedAttendance(context);
            }
            else
            {
               Console.WriteLine($"[Seeding] Skipped: Not in Development Environment.");
            }

            if (context.Users.Count() > 1) return; // If more than just our admin, skip rest

            var users = new User[]
            {
                // ... add other seed users here if requested ...
            };
            // Note: The original returned early, so users array was never reached if Any() was true.
            // Keeping it simple.

            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            
            context.SaveChanges();
        }

        private static void SeedEmployees(AppDbContext context)
        {
            int currentCount = context.Employees.Count();
            Console.WriteLine($"[Seeding] Current Employee Count: {currentCount}");
            if (currentCount >= 20) 
            {
                Console.WriteLine($"[Seeding] Sufficient employees exist. Skipping Employee Seed.");
                return; 
            }

            var random = new Random();
            var firstNames = new[] { "John", "Jane", "Mike", "Sarah", "David", "Emma", "Chris", "Lisa", "Tom", "Anna", "Robert", "Emily", "James", "Olivia", "Peter", "Grace", "Daniel", "Chloe", "Paul", "Mia" };
            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin" };
            var roles = (EmployeeRole[])Enum.GetValues(typeof(EmployeeRole));
            
            var existingCount = context.Employees.Count();
            var toAdd = 20 - existingCount;
            Console.WriteLine($"[Seeding] Adding {toAdd} dummy employees...");

            for (int i = 0; i < toAdd; i++)
            {
                var fn = firstNames[random.Next(firstNames.Length)];
                var ln = lastNames[random.Next(lastNames.Length)];
                var role = roles[random.Next(roles.Length)];
                
                var emp = new Employee
                {
                    FirstName = fn,
                    LastName = ln,
                    EmployeeNumber = $"EMP{existingCount + i + 100:000}",
                    IdNumber = $"{random.Next(100000, 999999)}{random.Next(1000, 9999)}08{random.Next(1, 9)}", // Fake ID
                    Email = $"{fn.ToLower()}.{ln.ToLower()}{random.Next(1,99)}@example.com",
                    Phone = $"08{random.Next(10000000, 99999999)}",
                    Role = role,
                    HourlyRate = random.Next(25, 150), // R25 - R150 per hour
                    Branch = random.NextDouble() > 0.5 ? "Johannesburg" : "Cape Town",
                    EmploymentType = EmploymentType.Permanent,
                    ShiftStartTime = new TimeSpan(7, 0, 0),
                    ShiftEndTime = new TimeSpan(16, 45, 0)
                };

                context.Employees.Add(emp);
            }
            context.SaveChanges(); // Save here to get Ids for Attendance
            Console.WriteLine($"[Seeding] Employees saved.");
        }

        private static void SeedAttendance(AppDbContext context)
        {
            Console.WriteLine($"[Seeding] Seeding Attendance History...");
            var employees = context.Employees.ToList();
            if (!employees.Any()) 
            {
                Console.WriteLine($"[Seeding] No employees found to seed attendance for.");
                return;
            }

            // Optimization: Fetch existing composite keys to memory to avoid N+1 DB calls
            // Note: Date comparison in SQL vs C# can be tricky, ensuring .Date stripping
            var existingKeys = context.AttendanceRecords
                .Select(a => new { a.EmployeeId, Date = a.Date })
                .AsEnumerable() // Execute query
                .Select(x => $"{x.EmployeeId}|{x.Date.Date:yyyyMMdd}") // Create efficient key
                .ToHashSet();

            Console.WriteLine($"[Seeding] Found {existingKeys.Count} existing attendance records.");

            var random = new Random();
            var startDate = DateTime.Today.AddDays(-60); // Cover "Last Month" fully
            var endDate = DateTime.Today;

            int recordsAdded = 0;

            foreach (var emp in employees)
            {
               // Reduced skip chance: 5%
                if (random.NextDouble() > 0.95) 
                {
                    Console.WriteLine($"[Seeding] Skipping random employee: {emp.DisplayName}");
                    continue; 
                }

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    // Skip weekends
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) continue;

                    // Check for existing record using In-Memory Hash Set
                    string key = $"{emp.Id}|{date:yyyyMMdd}";
                    if (existingKeys.Contains(key)) continue;

                    // 10% chance of being absent (no record)
                    if (random.NextDouble() > 0.90) continue;

                    // Determine Shift Times
                    var shiftStart = emp.ShiftStartTime ?? new TimeSpan(7, 0, 0);
                    var shiftEnd = emp.ShiftEndTime ?? new TimeSpan(16, 45, 0); // Default end

                    // Randomize Arrival: 80% On Time, 20% Late
                    TimeSpan arrival;
                    if (random.NextDouble() > 0.2)
                    {
                        // On Time (arrive 0-15 mins early)
                        arrival = shiftStart.Subtract(TimeSpan.FromMinutes(random.Next(0, 15)));
                    }
                    else
                    {
                        // Late (arrive 5-45 mins late)
                        arrival = shiftStart.Add(TimeSpan.FromMinutes(random.Next(5, 45)));
                    }

                    // Randomize Departure: 90% On Time/Late, 10% Early
                    TimeSpan departure;
                    if (random.NextDouble() > 0.1)
                    {
                        // Leave 0-30 mins after shift end
                        departure = shiftEnd.Add(TimeSpan.FromMinutes(random.Next(0, 30)));
                    }
                    else
                    {
                         // Leave Early (e.g. 15:30)
                         departure = shiftEnd.Subtract(TimeSpan.FromMinutes(random.Next(15, 60)));
                    }

                    var checkIn = date.Add(arrival);
                    var checkOut = date.Add(departure);
                    
                    var duration = (checkOut - checkIn).TotalHours;
                    if (duration < 0) duration = 0;

                    var record = new AttendanceRecord
                    {
                        EmployeeId = emp.Id,
                        Date = date,
                        CheckInTime = checkIn,
                        CheckOutTime = checkOut,
                        ClockInTime = arrival, // TimeSpan
                        Status = (arrival > shiftStart) ? AttendanceStatus.Late : AttendanceStatus.Present,
                        Branch = emp.Branch,
                        CachedHourlyRate = (decimal)emp.HourlyRate,
                        HoursWorked = duration
                    };

                    context.AttendanceRecords.Add(record);
                    recordsAdded++;
                }
            }
            
            if (recordsAdded > 0)
            {
                context.SaveChanges();
                Console.WriteLine($"[Seeding] Attendance Records Added and Saved: {recordsAdded}");
            }
            else
            {
                Console.WriteLine($"[Seeding] No new records needed to be added.");
            }
        }
    }
}
