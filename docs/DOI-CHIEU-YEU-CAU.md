# Đối chiếu với đề bài Topic 2

Đây là bản đối chiếu chức năng đã viết, không phải cam kết điểm số. Thầy/cô quyết định điểm sau khi xem báo cáo và demo.

| Yêu cầu | Vị trí / cách kiểm tra |
|---|---|
| Student, Teacher, Administrator | UserRole, cookie authentication, Authorize trên controller; ba nhóm menu |
| Đăng ký, đăng nhập, sửa hồ sơ, đổi mật khẩu | AccountController và Views/Account |
| Course name/level/description/duration/fee/syllabus | Model Course, Courses/Edit |
| Class schedule/room/capacity/teacher/status | CenterClass, ClassSession, Classes/Edit và Sessions |
| Tìm kiếm, lọc khóa, xem lớp | Home/Courses, Home/Course, phân trang 6 khóa |
| Đăng ký, chống trùng/đầy/đóng | EnrollmentService; kiểm tra server + unique index + transaction |
| Pending, Confirmed, Studying, Completed, Withdrawn | EnrollmentStatus, Enrollments/Status |
| Điểm danh từng buổi | Teaching/Attendance, bảng Attendances |
| Giữa kỳ/cuối kỳ và kết quả | Teaching/Scores, Learning/Results, 40/60 và ngưỡng đạt 5 |
| Nộp học phí, thu nhiều đợt, công nợ | Payments/Submit, Record, Review, PaymentService |
| Biên lai | Payments/Receipt; in hoặc lưu PDF qua trình duyệt |
| Quản trị người dùng/khóa/lớp/đăng ký/tiền | Các controller quản trị; bảo vệ vai trò ở máy chủ |
| Lịch tuần 0.5 | Timetable/Index, tuần trước/sau/chọn ngày, lọc theo vai trò |
| Dashboard 0.75 | Revenue theo khoảng ngày/tháng, occupancy, teacher hours, popularity, enrollment count |
| Thông báo 0.5 | NotificationService; buổi học 24h, học phí trong 3 ngày/quá hạn, công bố điểm |
| Xuất PDF hoặc Excel 0.25 | Reports/Export tạo Excel .xlsx cho danh sách, điểm danh, thanh toán |

## Đối chiếu dữ liệu mẫu

| Đề yêu cầu | Bộ mẫu |
|---|---:|
| 1 Administrator | 1 |
| 5 Teacher | 5 |
| 20 Student | 20 |
| 8 courses | 8, phủ Beginner đến Advanced và IELTS/TOEIC |
| 12 classes | 12 |
| 30 enrollments nhiều trạng thái | 36, đủ 5 trạng thái |
| 10 sessions có attendance | 24 buổi có 64 bản ghi; tổng cộng 72 buổi |
| Điểm của 10 students nhiều lớp | 18 học viên có cả hai điểm; mỗi lớp có 3 đăng ký |
| 20 payments nhiều trạng thái | 24, đủ Pending/Paid/Rejected |
| Lớp open/full/closed | LC-101 full; LC-111/112 closed; lớp còn lại open |
| Học viên có/chưa có lịch sử | student1–18 có; student19–20 chưa có |

## Điểm cần xác nhận với giảng viên

Phần Basic ghi tổng **8.0**, nhưng các mục bạn cung cấp cộng thành **7.0** (1.0 + 1.5 + 1.5 + 1.5 + 1.0 + 0.5). Advanced cộng đủ 2.0. Cần đối chiếu bản đề/rubric đầy đủ để biết 1.0 điểm cơ bản còn lại dành cho nội dung nào. Project bao phủ tất cả các mục chức năng được cung cấp, nhưng không tự gán mục chấm điểm còn thiếu.
