function updateViewCounts() {
    // Gọi AJAX để lấy dữ liệu mới về lượt xem từ server
    $.ajax({
        url: '@Url.Action("GetProductViews", "Product")', // Phương thức trong ProductController
        type: 'GET',
        success: function (data) {
            // Cập nhật lượt xem cho từng sản phẩm trong danh sách
            data.forEach(function (item) {
                $('#luotXem_' + item.IDContent).text(item.LuotXem + " Lượt Xem");
            });
        },
        error: function () {
            console.log("Lỗi khi cập nhật lượt xem.");
        }
    });
}

// Gọi hàm updateViewCounts mỗi 5 giây
//setInterval(updateViewCounts, 3000);
