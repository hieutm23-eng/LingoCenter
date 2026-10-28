# Kịch bản demo 12–15 phút

## 1. Giới thiệu (1 phút)

Nêu ba vai trò, bài toán quản lý khóa/lớp/đăng ký/điểm/học phí. Mở solution chỉ cấu trúc Models–Controllers–Views, service và DbContext. Mở ERD trong THIET-KE.md.

## 2. Học viên (3 phút)

Đăng nhập `student20@lingocenter.test`. Góc học tập trống là tình huống có chủ đích. Tìm IELTS/English, lọc trình độ, chuyển trang, mở chi tiết khóa, xem phòng/giáo viên/lịch. Chỉ ra LC-101 Full và LC-111/112 Closed.

Đăng ký LC-102 đang mở. Đăng ký xuất hiện Pending, học phí được chốt. Thử đăng ký lại cùng lớp: phải bị từ chối. Sang Học phí nhập số tiền nhỏ hơn học phí và mã giao dịch demo. Giao dịch là Pending, công nợ chưa giảm.

## 3. Quản trị (4 phút)

Đăng nhập admin. Mở Dashboard, chọn khoảng ngày; phân biệt tổng số enrollments với doanh thu chỉ Paid. Xem công nợ, occupancy, giờ dạy, mức quan tâm khóa học.

Vào Payments xác nhận giao dịch vừa gửi. Công nợ giảm đúng số tiền, có biên lai. Mở in/lưu PDF. Vào Enrollments chuyển Pending → Confirmed → Studying. Không chuyển thẳng Pending → Completed.

Tạo một khóa/lớp thử (không sửa dữ liệu gốc nếu không cần), phân công giáo viên, thêm buổi trong tuần; thử thêm cùng giờ/phòng để minh họa chặn trùng. Xuất Excel danh sách theo lớp và báo cáo thanh toán, mở file bằng Excel.

## 4. Giáo viên (3 phút)

Đăng nhập giáo viên được phân công lớp đó hoặc teacher1 với LC-101. Mở lịch tuần. Vào buổi đã diễn ra để điểm danh; không điểm danh trước thời điểm bắt đầu. Nhập điểm thang 10, lưu nháp, sau đó công bố. Thử điểm 11 để cho thấy kiểm tra dữ liệu.

## 5. Quay lại học viên (2 phút)

Với lớp demo LC-101 của teacher1, `student1`, `student2`, `student3` có enrollment. Đăng nhập một tài khoản tương ứng để xem điểm đã công bố, công thức 40/60 và chi tiết attendance. Với student20, lớp vừa Confirmed/Studying sẽ xuất hiện trong lịch tuần.

Vào thông báo để xem nhắc học phí/điểm. Chọn một buổi trong 24 giờ tới nếu muốn demo nhắc lịch; thời điểm seed có thể khác thời điểm thuyết trình.

## Checklist kiểm tra trước khi nộp

- Đăng ký email trùng, sai mật khẩu, sửa hồ sơ, đổi mật khẩu và đăng nhập lại.
- Truy cập URL quản trị bằng học viên bị chặn; giáo viên không sửa lớp người khác; học viên không xem biên lai người khác.
- Mở/đầy/đóng; duplicate enrollment; lịch giao nhau; sĩ số không thể giảm thấp hơn số đã đăng ký.
- Khoản âm, bằng 0, vượt nợ bị từ chối; Pending chưa giảm nợ; không xác nhận một payment hai lần.
- Điểm ngoài 0–10 bị chặn; nháp không lộ; công bố mới hiển thị; completed cần đủ điểm.
- Xuất đúng bộ lọc; tiếng Việt trong Excel; biên lai in không chứa menu.
- Ảnh màn hình thật của từng vai trò trên máy demo; chỉ đưa ảnh dữ liệu thật đã chạy vào báo cáo chính thức.

## Các câu hỏi nên chuẩn bị

Vì sao enrollment lưu AgreedFee? Vì sao Full được tính thay vì lưu? Vì sao không xóa payment Paid? Session khác database như thế nào? Vì sao cần transaction/unique index khi kiểm tra duplicate đã có trong C#? Giáo viên được giới hạn theo TeacherId ở đâu? Cách chuyển sang máy khác và SQL Server khác?
