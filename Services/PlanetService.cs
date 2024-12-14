using App.Models;

namespace App.Services
{
    public class PlanetService : List<PlanetModel>
    {
        public PlanetService()
        {
            Add(new PlanetModel()
            {
                Id = 1,
                Name = "Mercury",
                VnName = "Sao Thủy",
                content = "hành tinh nhỏ nhất và gần Mặt Trời nhất trong tám hành tinh thuộc hệ Mặt Trời, với chu kỳ quỹ đạo bằng khoảng 88 ngày Trái Đất. Nhìn từ Trái Đất, hành tinh hiện lên với chu kỳ giao hội trên quỹ đạo bằng xấp xỉ 116 ngày, và nhanh hơn hẳn những hành tinh khác. Tốc độ chuyển động nhanh này đã khiến người La Mã đặt tên hành tinh là Mercurius, vị thần liên lạc và đưa tin một cách nhanh chóng. Trong thần thoại Hy Lạp tên của vị thần này là Hermes (Ερμής). Tên tiếng Việt của hành tinh này dựa theo tên do Trung Quốc đặt, chọn theo hành thủy trong ngũ hành."
            });

            Add(new PlanetModel()
            {
                Id = 2,
                Name = "Earth",
                VnName = "Trái Đất",
                content = "hành tinh thứ ba tính từ Mặt Trời, đồng thời cũng là hành tinh lớn nhất trong các hành tinh đất đá của hệ Mặt Trời xét về bán kính, khối lượng và mật độ của vật chất. Trái Đất còn được biết tên với các tên gọi \"hành tinh xanh\", là nhà của hàng triệu loài sinh vật, trong đó có con người và cho đến nay nó là nơi duy nhất trong vũ trụ được biết đến là có sự sống. Hành tinh này được hình thành cách đây khoảng 4,55 tỷ năm và sự sống xuất hiện trên bề mặt của nó khoảng 1 tỷ năm trước. Kể từ đó, sinh quyển, khí quyển của Trái Đất và các điều kiện vô cơ khác đã thay đổi đáng kể, tạo điều kiện thuận lợi cho sự phổ biến của các vi sinh vật ưa khí cũng như sự hình thành của tầng ôzôn-lớp bảo vệ quan trọng, cùng với từ trường của Trái Đất, đã ngăn chặn các bức xạ có hại và chở che cho sự sống. Các đặc điểm vật lý của Trái Đất cũng như lịch sử địa lý hay quỹ đạo, cho phép sự sống tồn tại trong thời gian qua. Người ta ước tính rằng Trái Đất chỉ còn có thể hỗ trợ sự sống thêm 1,5 tỷ năm nữa, trước khi kích thước của Mặt Trời tăng lên (trở thành sao khổng lồ đỏ) và tiêu diệt hết sự sống."
            });
        }
    }
}