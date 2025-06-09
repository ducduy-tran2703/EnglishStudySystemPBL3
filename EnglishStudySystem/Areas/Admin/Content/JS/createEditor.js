// createEditor.js
$(document).ready(function () {
    // Auto-generate username from email
    $('#Email').on('blur', function () {
        const email = $(this).val();
        if (email && $('#UserName').val() === '') {
            const username = email.split('@')[0];
            $('#UserName').val(username);
        }
    });

    // Password strength indicator

    function updateStrengthIndicator(strength) {
        const indicator = $('#password-strength');
        const text = $('#password-strength-text');

        indicator.removeClass('bg-danger bg-warning bg-success');
        text.removeClass('text-danger text-warning text-success');

        if (strength === 0) {
            indicator.css('width', '0%');
            text.text('');
        } else if (strength <= 2) {
            indicator.addClass('bg-danger').css('width', '40%');
            text.addClass('text-danger').text('Yếu');
        } else if (strength === 3) {
            indicator.addClass('bg-warning').css('width', '70%');
            text.addClass('text-warning').text('Trung bình');
        } else {
            indicator.addClass('bg-success').css('width', '100%');
            text.addClass('text-success').text('Mạnh');
        }
    }
});