# Kết quả kiểm tra ngày 04/10/2026

## Đã thực hiện

- MSBuild biên dịch project ASP.NET MVC 5 thành công, không còn cảnh báo ở lần build kiểm tra.
- Đã giải nén gói giao sang thư mục khác, build solution ở cấu hình Release bằng thư viện cục bộ và chạy lại đủ 42 kiểm tra thành công. Đây là kiểm tra đóng gói và đường dẫn tương đối, chưa phải kiểm tra trên máy vật lý khác.
- ASP.NET compiler biên dịch toàn bộ `.cshtml` thành công.
- 42 kiểm tra trong TestRunner qua: hash/verify/salt mật khẩu; điểm tổng kết/thiếu điểm; tiền Paid/Pending/Rejected; chỗ trống/đầy/đóng; validation điểm; cấu trúc gói XLSX và ngăn công thức; AntiForgery trên mọi POST; khai báo quyền admin; EF model SQL Server.
- File XLSX do mã ứng dụng tạo được mở bằng bộ đọc độc lập: đúng tiếng Việt, số hàng/cột, chuỗi bắt đầu bằng dấu bằng không bị coi là công thức.
- EF dựng mô hình và tạo DDL SQL Server offline thành công, gồm các quan hệ khóa ngoại.
- Kiểm tra fixture dữ liệu: 26 users, 8 courses, 12 classes, 36 unique student/class pairs; 18 học viên có lịch sử và 2 chưa có; 72 sessions, 64 attendance records, 24 payments.
- Render thật Razor trang Login qua ASP.NET runtime trong trình kiểm tra không dùng IIS; xác minh tiếng Việt và bố cục ở 1440px, 390px.
- Render thật Razor Dashboard bằng fixture riêng để kiểm tra bố cục. Số tiền và lớp trong ảnh này là dữ liệu kiểm tra giao diện, không phải kết quả đọc từ SQL.

## Chưa thực hiện thành công

IIS Express không khởi động trong môi trường thực thi: HTTP.sys/ULATQ trả `0x80070005 Access denied`. SQL Server Express đang chạy nhưng kết nối Windows Authentication trả `Failed to generate SSPI context`, kể cả khi đã cấp quyền mạng cho thao tác.

Vì vậy **chưa xác minh end-to-end** các thao tác ghi dữ liệu, tạo database bằng EF, đăng nhập với SQL, xác nhận thanh toán, chạy kiểm tra tích hợp hoặc tình huống hai request đồng thời trên SQL thật. Không coi build thành công là bằng chứng các luồng này đã chạy thành công.

## Bước xác minh còn lại trên máy người dùng

1. Kết nối `.\SQLEXPRESS` bằng SSMS và tài khoản Windows có quyền.
2. Mở solution, Ctrl+F5; đăng nhập admin và kiểm tra số lượng.
3. Chạy các tình huống trong KICH-BAN-DEMO.md.
4. Chạy project Tests với tham số `--integration` trên SQL instance đã cấu hình. Bộ kiểm tra dùng database có tên GUID riêng và dọn database thử của chính nó.
5. Đặc biệt kiểm tra đồng thời đăng ký chỗ cuối, cập nhật lịch và nhập attendance từ hai cửa sổ; bộ kiểm tra offline không đo tải/đồng thời.

Ảnh minh họa trong `docs/images/` phục vụ xem thiết kế. Chụp lại ứng dụng đang chạy với SQL thật cho phần minh chứng của báo cáo cuối kỳ.
