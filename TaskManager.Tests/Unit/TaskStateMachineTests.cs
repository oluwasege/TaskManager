using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.StateMachine;
using Xunit;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Tests.Unit
{
    public class TaskStateMachineTests
    {
        private readonly TaskStateMachine _stateMachine;

        public TaskStateMachineTests()
        {
            _stateMachine = new TaskStateMachine();
        }

        [Fact]
        public void CanTransition_FromPendingToInProgress_ReturnsTrue()
        {
            // Arrange
            var currentStatus = TaskStatus.Pending;
            var newStatus = TaskStatus.InProgress;

            // Act
            var result = _stateMachine.CanTransition(currentStatus, newStatus);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanTransition_FromPendingToCompleted_ReturnsFalse()
        {
            // Arrange
            var currentStatus = TaskStatus.Pending;
            var newStatus = TaskStatus.Completed;

            // Act
            var result = _stateMachine.CanTransition(currentStatus, newStatus);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Transition_WithValidTransition_ReturnsNewStatus()
        {
            // Arrange
            var currentStatus = TaskStatus.Pending;
            var newStatus = TaskStatus.InProgress;

            // Act
            var result = _stateMachine.Transition(currentStatus, newStatus);

            // Assert
            Assert.Equal(newStatus, result);
        }

        [Fact]
        public void Transition_WithInvalidTransition_ThrowsException()
        {
            // Arrange
            var currentStatus = TaskStatus.Pending;
            var newStatus = TaskStatus.Completed;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _stateMachine.Transition(currentStatus, newStatus));
        }
    }
}
