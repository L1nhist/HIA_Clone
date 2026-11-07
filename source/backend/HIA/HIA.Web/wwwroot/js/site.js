function changePassword(currentPassId, newPassId, confirmPassId, commonId, action) {
    $(`#${currentPassId}Error`).text('');
    $(`#${newPassId}Error`).text('');
    $(`#${confirmPassId}Error`).text('');
    $(`#${commonId}Error`).text('');

    const data = {
        currentPassword: $(`#${currentPassId}`).val(),
        newPassword: $(`#${newPassId}`).val(),
        confirmPassword: $(`#${confirmPassId}`).val(),
    };
    $.ajax({
        url: action,
        method: 'POST',
        data: data,
        cache: false,
        success: function (data) {
            if (data?.code !== 200) {
                const currentPassError = data?.data.find(x => x.key === currentPassId)?.errormessage;
                $(`#${currentPassId}Error`).text(currentPassError);

                const newPassError = data?.data.find(x => x.key === newPassId)?.errormessage;
                $(`#${newPassId}Error`).text(newPassError);

                const confirmPassError = data?.data.find(x => x.key === confirmPassId)?.errormessage;
                $(`#${confirmPassId}Error`).text(confirmPassError);

                const commonError = data?.data.find(x => x.key === commonId)?.errormessage;
                $(`#${commonId}Error`).html(commonError);
                return;
            }
            $('#changePassModal').modal('hide');
            $(document.body).removeClass("modal-open");
            $(".modal-backdrop").remove();

            $.toast({
                heading: 'Thành công',
                text: 'Thay đổi mật khẩu thành công!',
                position: 'top-center',
                loaderBg: '#ff6849',
                icon: 'success',
                hideAfter: 2000,
                stack: 6
            });
        },
        fail: function (jqXHR, textStatus) {
            alert("Request failed: " + textStatus);
        }
    });
}