// Toggle sidebar on mobile
document.querySelector('.sidebar-toggle').addEventListener('click', function () {
    document.querySelector('.admin-sidebar').classList.toggle('show');
});

// Close sidebar when clicking outside
document.addEventListener('click', function (e) {
    const sidebar = document.querySelector('.admin-sidebar');
    const toggleBtn = document.querySelector('.sidebar-toggle');

    if (!sidebar.contains(e.target) && !toggleBtn.contains(e.target) && window.innerWidth < 992) {
        sidebar.classList.remove('show');
    }
});

// Activate tooltips
const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
});

// Highlight active menu item
const currentPath = window.location.pathname;
document.querySelectorAll('.sidebar-menu .nav-link').forEach(link => {
    if (link.getAttribute('href') === currentPath) {
        link.classList.add('active');
    }
});