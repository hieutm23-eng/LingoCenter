# Hướng dẫn chạy và chuyển máy

## 1. Mỗi máy dùng database riêng

Copy toàn bộ thư mục LingoCenter. Mỗi máy Windows cần Visual Studio với workload ASP.NET and web development, .NET Framework 4.8 Developer/Targeting Pack, IIS Express và một SQL Server instance. Máy đã có các thành phần này không cần mạng để tải thư viện project.

Mở solution, chọn LingoCenter làm startup project, chỉnh connection string trong `LingoCenter/Web.config`, nhấn Ctrl+F5.

SQL Express:

```xml
<add name="CenterDb"
 connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=LingoCenterDemo;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
 providerName="System.Data.SqlClient" />
```

LocalDB:

```xml
<add name="CenterDb"
 connectionString="Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=LingoCenterDemo;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
 providerName="System.Data.SqlClient" />
```

LocalDB phải được cài đặt; đổi connection string không tự cài LocalDB. SSMS là công cụ quản lý, không thay thế dịch vụ SQL Server. Kiểm tra bạn kết nối được đúng server trong SSMS bằng Windows Authentication trước khi chạy ứng dụng.

Tài khoản Windows chạy ứng dụng cần quyền tạo database trong lần đầu. Nếu không có quyền, nhờ người quản trị SQL tạo schema bằng script đi kèm và cấp quyền đọc/ghi database. Không dùng SQL credentials của người khác hoặc ghi mật khẩu thật vào bài nộp.

## 2. Hai cách khởi tạo database

**Cách A — nên dùng:** chạy ứng dụng lần đầu. `CreateDatabaseIfNotExists` tạo bảng, unique indexes và gọi `DemoInitializer.Seed`. Lịch demo tương đối theo tuần hiện tại. Chạy lần hai không tạo thêm dữ liệu trùng và không xóa dữ liệu đã sửa.

**Cách B — SSMS:** chỉ khi chưa có database `LingoCenterDemo`, mở và chạy `Database/Install-Demo.sql`. Script tạo schema và dữ liệu mẫu, có mật khẩu hash tương thích ứng dụng. Sau đó chạy ứng dụng với đúng connection string. Không chạy cả hai cách cho cùng database.

Chạy `Database/Verify-Demo.sql` để đối chiếu số lượng, lớp đầy/đóng, học viên chưa đăng ký, số buổi có điểm danh. File SQL là phương án triển khai thay thế, không phải bản sao lưu `.bak` của database đã chạy thực tế.

Nếu lần khởi tạo bị gián đoạn, không ghi đè tùy tiện: đổi `Initial Catalog` sang một tên mới chưa tồn tại rồi chạy lại. Nếu muốn đổi mô hình sau này, cần EF migrations và sao lưu trước khi cập nhật schema.

## 3. Nhiều máy dùng chung dữ liệu

Triển khai **một** bản ứng dụng lên Windows IIS; các máy còn lại truy cập URL bằng Chrome/Edge. Máy khách không cần Visual Studio hay SQL Server.

1. Trên máy chủ, bật IIS, ASP.NET 4.x, .NET Extensibility, ISAPI Extensions/Filters; cài .NET Framework 4.8.
2. Trong Visual Studio, Publish project ra Folder. Copy thư mục publish lên máy chủ.
3. Tạo Application Pool dùng .NET CLR v4.0, Integrated pipeline. Tạo IIS site/application trỏ tới thư mục publish.
4. Cấu hình connection string tới SQL Server của máy chủ. Nếu Integrated Security, tài khoản application pool cần được SQL Server cấp quyền database; quyền Windows của người phát triển không tự áp dụng cho IIS.
5. Dùng tài khoản triển khai có quyền tạo schema ở lần cài; sau đó tài khoản chạy ứng dụng chỉ cần quyền dữ liệu phù hợp. Với SQL trên máy khác, cấu hình TCP, firewall và tài khoản kết nối cùng người quản trị hệ thống.
6. Chọn HTTPS, tắt debug, bật cookie chỉ dùng HTTPS, đổi mật khẩu demo trước khi sử dụng ngoài buổi trình diễn. Không dùng `TrustServerCertificate=True` như cấu hình mặc định cho hệ thống Internet có chứng chỉ chuẩn.
7. Máy khách truy cập tên miền hoặc địa chỉ máy chủ đã cấu hình. Đồng hồ nghiệp vụ hiển thị giờ Việt Nam.

Không mở trực tiếp file `.cshtml` bằng trình duyệt; đó là mã Razor cần ASP.NET xử lý. Không đặt file MDF lên thư mục chia sẻ để nhiều máy cùng mở. Nếu di chuyển dữ liệu hiện có, dùng backup/restore SQL Server theo phiên bản tương thích; copy source chỉ mang theo mã và dữ liệu mẫu, không mang mọi thay đổi trong database đang dùng.

## 4. Xử lý lỗi thường gặp

| Hiện tượng | Kiểm tra |
|---|---|
| Không mở được project web | Workload ASP.NET and web development, .NET Framework 4.8 targeting pack |
| Thiếu DLL khi build | Đã giải nén toàn bộ `packages/` cạnh file solution chưa? |
| SQL network/instance error | Tên instance, dịch vụ SQL đang chạy, kết nối SSMS |
| Login failed / SSPI | Chạy Visual Studio bằng tài khoản Windows có quyền SQL; kiểm tra Windows Authentication và quyền database |
| Database tồn tại nhưng thiếu bảng | Dùng database mới hoặc hoàn thành cài schema; không ghi đè database có dữ liệu |
| IIS Express access denied | Kiểm tra IIS Express/HTTP.sys và cổng địa phương; trong VS chọn lại IIS Express |
| Lịch demo không nằm tuần hiện tại | Lịch chốt ở lần seed; chọn tuần tương ứng trong lịch hoặc tạo demo bằng tên database mới |
| Giao dịch chưa làm giảm công nợ | Pending phải được quản trị xác nhận thành Paid |
| Không hiện điểm cho học viên | Giáo viên phải bấm Lưu & công bố điểm |

## 5. Kiểm tra tự động

Solution đã chứa project `LingoCenter.Tests`. Chọn project kiểm tra làm Startup Project rồi chạy; chuyển lại LingoCenter khi muốn chạy website. Không truyền tham số: chạy bộ kiểm tra không cần SQL. Truyền `--integration` trong Debug command line arguments để tạo database thử `LingoCenterTest_<GUID>` trên `.\SQLEXPRESS`, kiểm tra nghiệp vụ và xóa đúng database thử sau khi hoàn tất. Không tác động database demo.

Có thể đặt biến môi trường `LINGO_TEST_SQLSERVER` thành `(LocalDB)\MSSQLLocalDB` hoặc tên SQL server khác trước khi chạy kiểm tra tích hợp. Không báo là đã kiểm tra tích hợp thành công khi chỉ chạy chế độ không SQL.
