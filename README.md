# LingoCenter — English Language Center Management System

Đồ án Web Application Development, Topic 2. ASP.NET MVC 5, .NET Framework 4.8, Razor, Entity Framework 6 và SQL Server. Giao diện tiếng Việt; trạng thái giữ tên theo đề bài.

## Chạy bằng Visual Studio trên máy của bạn

1. Giải nén **toàn bộ** thư mục, gồm `LingoCenter`, `packages`, `Database`, `docs`. Không mở solution trực tiếp trong ZIP.
2. Máy cần Windows, Visual Studio 2022 có workload **ASP.NET and web development**, **.NET Framework 4.8 targeting pack**, IIS Express và SQL Server Express hoặc LocalDB.
3. Mở `LingoCenter.sln`, đặt project LingoCenter làm **Startup Project**.
4. Mặc định `Web.config` dùng `Data Source=.\SQLEXPRESS`, database `LingoCenterDemo`, đăng nhập Windows. Nếu dùng LocalDB, đổi Data Source thành `(LocalDB)\MSSQLLocalDB`. Nếu dùng instance khác, nhập đúng tên server trong SSMS.
5. Nhấn **Ctrl+F5**. Lần đầu Entity Framework tự tạo database, bảng và dữ liệu demo. Không cần chạy SQL trước.
6. Đăng nhập bằng các tài khoản dưới đây. Đợi vài giây trong lần chạy đầu để tạo dữ liệu.

**Thư viện đã đi kèm trong `packages/`, tham chiếu bằng đường dẫn tương đối. Có thể build không cần tải NuGet; không đổi tên hoặc xóa thư mục này.**

## Tài khoản demo

| Vai trò | Email | Mật khẩu |
|---|---|---|
| Quản trị | admin@lingocenter.test | Lingo@2026! |
| Giáo viên | teacher1@lingocenter.test đến teacher5@lingocenter.test | Lingo@2026! |
| Học viên | student1@lingocenter.test đến student20@lingocenter.test | Lingo@2026! |

`student19` và `student20` chưa có đăng ký. `LC-101` đầy; `LC-111`, `LC-112` đóng. Đây là dữ liệu giả lập phục vụ đồ án.

## Chức năng

- Tài khoản, ba vai trò, hồ sơ, đổi mật khẩu, PBKDF2 có salt, tạm khóa khi đăng nhập sai.
- CRUD khóa học, đề cương, trình độ, thời lượng, học phí; ẩn khóa học thay cho xóa lịch sử.
- Lớp, giáo viên, phòng, sĩ số và buổi học; chặn trùng lịch giáo viên/phòng/học viên.
- Tìm kiếm, lọc, phân trang, xem lịch lớp, đăng ký; chặn trùng, đầy, đóng và trùng lịch.
- Quản lý các trạng thái đăng ký và đăng ký thay học viên.
- Giáo viên điểm danh, nhập điểm, lưu nháp và công bố điểm trong lớp được phân công.
- Học phí nhiều đợt, xác nhận/từ chối giao dịch, công nợ và biên lai. In/lưu biên lai PDF qua trình duyệt.
- Lịch tuần, dashboard doanh thu theo kỳ, tỷ lệ lấp đầy, giờ dạy và mức quan tâm khóa học.
- Thông báo trong ứng dụng về buổi học, hạn học phí và điểm công bố.
- Xuất Excel `.xlsx` danh sách đăng ký, điểm danh, thanh toán với bộ lọc lớp/ngày.

Thanh toán là **quy trình thủ công có quản trị viên xác nhận**, không phải cổng ngân hàng tự động. Thông báo cập nhật khi học viên truy cập, không gửi email/SMS và không chạy nền khi ứng dụng tắt.

## Dữ liệu demo

26 tài khoản (1 quản trị, 5 giáo viên, 20 học viên), 8 khóa học, 12 lớp, 36 đăng ký, 72 buổi học, 64 bản ghi điểm danh trên 24 buổi, điểm giữa/cuối kỳ cho 18 học viên và 24 giao dịch đủ trạng thái Pending/Paid/Rejected. Lịch được tính theo tuần chạy lần đầu.

Database có sẵn được giữ nguyên. Không tự xóa database khi mô hình thay đổi. Muốn bộ demo mới: đổi `Initial Catalog` sang tên database mới. Thay đổi mô hình về sau cần migrations hoặc cập nhật schema có kiểm soát.

## Tài liệu

- `docs/HUONG-DAN.md`: chạy trên máy khác và triển khai dùng chung.
- `docs/THIET-KE.md`: kiến trúc, ERD, phân quyền, liên hệ bài lab.
- `docs/DOI-CHIEU-YEU-CAU.md`: đối chiếu yêu cầu chấm điểm.
- `docs/KICH-BAN-DEMO.md`: trình tự demo, tình huống kiểm thử.
- `docs/KET-QUA-KIEM-TRA.md`: kết quả thực tế, phần chưa xác minh.
- `Database/Install-Demo.sql`: cách tạo database bằng SSMS thay cho EF; chỉ dùng khi database chưa tồn tại.
- `Database/Verify-Demo.sql`: kiểm tra số lượng và trạng thái dữ liệu.
- `Tests/TestRunner.cs`: bộ kiểm tra tự động; có chế độ kiểm tra tích hợp trên database thử riêng.

## Trạng thái kiểm tra

Build C# và biên dịch toàn bộ Razor đã qua. **42 kiểm tra tự động không cần SQL đã qua.** Đăng nhập đã render bằng ASP.NET và kiểm tra desktop/mobile. Dashboard render từ Razor với dữ liệu fixture để kiểm tra bố cục.

**Chưa xác nhận end-to-end với SQL Server/IIS**: môi trường thực thi chặn HTTP.sys và xác thực Windows (SSPI). Chạy Ctrl+F5 trên máy người dùng để kiểm chứng bước còn lại; chi tiết trong tài liệu kiểm tra.
