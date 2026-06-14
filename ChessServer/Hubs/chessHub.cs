using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChessServer.Hubs;

public class ChessHub : Hub
{
    // Cuốn sổ tay: Tên Phòng -> [ConnectionId người lập phòng, Màu đã chọn]
    private static ConcurrentDictionary<string, string[]> roomData = new ConcurrentDictionary<string, string[]>();

    // Trả về TRUE nếu vào phòng thành công, FALSE nếu bị trùng màu
    public async Task<bool> JoinRoom(string roomName, string playerName, string avatar, string color)
    {
        if (roomData.ContainsKey(roomName))
        {
            string existingId = roomData[roomName][0];
            string existingColor = roomData[roomName][1];

            if (existingColor == color)
            {
                // Trùng phe -> Từ chối thẳng tay người vào sau!
                return false; 
            }
            else
            {
                // Hợp lệ -> Cho người thứ 2 vào phòng
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                
                // Báo cho người tạo phòng (đang chờ) biết để bắt đầu game
                await Clients.Client(existingId).SendAsync("OpponentJoined", playerName, avatar, color);
                
                // Phòng đã đủ 2 người, gạch tên khỏi sổ chờ
                roomData.TryRemove(roomName, out _);
                return true; 
            }
        }
        else
        {
            // Người đầu tiên lập phòng
            roomData.TryAdd(roomName, new string[] { Context.ConnectionId, color });
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            return true;
        }
    }

    // Dọn dẹp sổ tay nếu người đang chờ đột ngột tắt web hoặc F5
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var ghostRoom = roomData.FirstOrDefault(x => x.Value[0] == Context.ConnectionId).Key;
        if (ghostRoom != null)
        {
            roomData.TryRemove(ghostRoom, out _);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendProfile(string roomName, string playerName, string avatar, string color)
    {
        await Clients.OthersInGroup(roomName).SendAsync("ReceiveProfile", playerName, avatar, color);
    }

    public async Task SendMove(string roomName, object moveData, string fen)
    {
        await Clients.OthersInGroup(roomName).SendAsync("ReceiveMove", moveData, fen);
    }
    // Xử lý khi có người yêu cầu chơi ván mới (Rematch)
    public async Task RequestRematch(string roomName)
    {
        // Ra lệnh "ResetGame" cho cả 2 máy trong phòng
        await Clients.Group(roomName).SendAsync("ResetGame");
    }

    // Xử lý khi có người giơ cờ trắng
    public async Task Surrender(string roomName, string loserName)
    {
        // Thông báo cho cả phòng biết ai là người đầu hàng
        await Clients.Group(roomName).SendAsync("PlayerSurrendered", loserName);
    }
}