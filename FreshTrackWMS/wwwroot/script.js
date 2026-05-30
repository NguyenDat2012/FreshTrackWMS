const toggleBtn = document.getElementById('sidebarToggle');
const sidebar = document.getElementById('sidebar');

// Bấm nút để bật/tắt menu
toggleBtn?.addEventListener('click', (e) => {
    e.stopPropagation(); // Ngăn không cho click ăn ra ngoài body
    sidebar?.classList.toggle('active');
});

// Click ra ngoài khoảng trống để đóng menu (dành cho màn hình nhỏ)
document.addEventListener('click', (e) => {
    if (window.innerWidth <= 768 && sidebar?.classList.contains('active')) {
        // Nếu click không trúng sidebar và không trúng nút toggle thì đóng
        if (!sidebar.contains(e.target) && !toggleBtn.contains(e.target)) {
            sidebar.classList.remove('active');
        }
    }
});