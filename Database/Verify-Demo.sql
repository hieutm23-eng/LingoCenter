USE LingoCenterDemo;
SELECT Role,COUNT(*) AS Accounts FROM dbo.Users GROUP BY Role;
SELECT 'Courses' AS Entity,COUNT(*) AS Total FROM dbo.Courses UNION ALL SELECT 'Classes',COUNT(*) FROM dbo.CenterClasses UNION ALL SELECT 'Enrollments',COUNT(*) FROM dbo.Enrollments UNION ALL SELECT 'Sessions',COUNT(*) FROM dbo.ClassSessions UNION ALL SELECT 'Attendance',COUNT(*) FROM dbo.Attendances UNION ALL SELECT 'Payments',COUNT(*) FROM dbo.Payments;
SELECT Status,COUNT(*) AS Enrollments FROM dbo.Enrollments GROUP BY Status;
SELECT Status,COUNT(*) AS Payments FROM dbo.Payments GROUP BY Status;
SELECT COUNT(DISTINCT ClassSessionId) AS SessionsWithAttendance FROM dbo.Attendances;
SELECT COUNT(DISTINCT StudentId) AS StudentsWithBothScores FROM dbo.Enrollments WHERE Midterm IS NOT NULL AND Final IS NOT NULL;
SELECT c.Code,c.Capacity,COUNT(e.Id) AS Occupied,CASE WHEN c.Status=1 THEN 'Closed' WHEN COUNT(e.Id)>=c.Capacity THEN 'Full' ELSE 'Open' END AS Availability FROM dbo.CenterClasses c LEFT JOIN dbo.Enrollments e ON e.CenterClassId=c.Id AND e.Status<>4 GROUP BY c.Code,c.Capacity,c.Status;
SELECT u.FullName,u.Email FROM dbo.Users u WHERE u.Role=0 AND NOT EXISTS(SELECT 1 FROM dbo.Enrollments e WHERE e.StudentId=u.Id);
