using ChessServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm dịch vụ SignalR
builder.Services.AddSignalR();

// 2. Cấu hình CORS (Mở cửa cho Frontend gọi vào)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .SetIsOriginAllowed((host) => true) 
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

// Kích hoạt CORS
app.UseCors("AllowAll");

// Đăng ký đường dẫn cho Trạm thu phát
app.MapHub<ChessHub>("/chessHub");

// Chạy server
app.Run();