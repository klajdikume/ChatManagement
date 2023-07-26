using ChatManagement.Controllers;
using ChatManagement.IServices;
using ChatManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatManagement.Test
{
    public class ChatControllerTests
    {
        [Fact]
        public void ShouldCreateSessionSuccessfully()
        {
            // Arrange
            var rabbitMQServiceMock = new Mock<RabbitMQService>();
            var chatManagementServiceMock = new Mock<IChatManagementService>();
            var controller = new ChatController(rabbitMQServiceMock.Object, chatManagementServiceMock.Object);

            chatManagementServiceMock.Setup(x => x.RefuseChat()).Returns(false);

            // Act
            IActionResult result = controller.CreateSession();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<Guid>(okResult.Value);
        }
        [Fact]
        public void ShouldFailToCreateSessionWhenNoAgentsAvailable()
        {
            // Arrange
            var rabbitMQServiceMock = new Mock<RabbitMQService>();
            var chatManagementServiceMock = new Mock<IChatManagementService>();
            var controller = new ChatController(rabbitMQServiceMock.Object, chatManagementServiceMock.Object);

            chatManagementServiceMock.Setup(x => x.RefuseChat()).Returns(true);

            // Act
            IActionResult result = controller.CreateSession();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No Agents available at the moment.", badRequestResult.Value);
        }
    }
}
