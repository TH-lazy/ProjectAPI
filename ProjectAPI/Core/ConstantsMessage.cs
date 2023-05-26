namespace ProjectAPI.Core
{
    public static class ConstantsMessage
    {
        public static string BADREQUEST = "Đã có lỗi xảy ra. Vui lòng liên hệ DEV để được hỗ trợ";
        public static string AUTHORIZED = "Xác thực tài khoản thành công";
        public static string UNAUTHORIZED = "Xác thực tài khoản không thành công";
        public static string FORBIDDEN = "Không có quyền truy cập";

        public static string TOKEN_EXPIRED = "Phiên làm việc đã hết hạn";
        public static string TOKEN_INVALID = "Thông tin xác thực không hợp lệ";

        public static string DATA_INVALID = "Dữ liệu không hợp lệ";
        public static string NODATA = "Không tìm thấy dữ liệu";
        public static string HASDATA = "Dữ liệu với SL: {0}";

        public static string GENERATE_SUCCESS = "Khởi tạo thành công";
        public static string GENERATE_FAIL = "Khởi tạo không thành công";
    }
}