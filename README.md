# 💬 Lập trình ứng dụng Chat (Console) via TCP

## 📌 Giới thiệu
Ứng dụng chat sử dụng TCP Socket trong C#, cho phép nhiều client kết nối đến server và gửi/nhận tin nhắn theo thời gian thực qua command line.

---

## ⚙️ Cách chạy

### 🔹 1. Chạy Server
- Mở project Server
- Nhấn **F5**

 Server chạy
### 🔹 2. Chạy Client
- Mở project Client
- Nhấn **F5**
- Nhập tên người dùng (tối đa 20 ký tự)

---

## Cách sử dụng

### Chat toàn bộ

Hello moi nguoi

 Tin nhắn sẽ gửi tới tất cả người dùng

---

### Chat riêng (ghim người)

/to <ten>

Ví dụ:

/to huy
---

### Quay lại chat chung

/to ALL
---

### Xem danh sách online

/list
---

### Gửi tin nhắn riêng trực tiếp

/send <ten> <noi dung>
---

### Kiểm tra user / nhóm

/check <ten>
---

### Thoát

/exit

### Tạo nhóm

/create <ten_nhom>

Ví dụ:

/create team1

### Tham gia nhóm

/join <ten_nhom>

Ví dụ:

/join team1

### Rời nhóm

/leave <ten_nhom>

Nếu nhóm không còn ai → tự động xóa

### Chat trong nhóm

/to <ten_nhom>

Ví dụ:

/to team1

Hello anh em
---
#### Hiển thị trên Server
Khi user vào:

[HE THONG]: huy da tham gia phong chat.

Khi user thoát:

[HE THONG]: huy da roi phong chat.

Khi chat:

12:30:15 - huy: Hello

## ⚠️ Lưu ý
- Server tối đa 10 người
- Username không được trùng
- Phải chạy Server trước Client

---

## 🧠 Công nghệ
- C#
- TCP Socket
- Multithreading

---
