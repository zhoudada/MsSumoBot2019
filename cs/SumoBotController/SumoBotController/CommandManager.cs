using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System;

namespace SumoBotController
{
    public class CommandManager
    {
        public delegate Task CommandEvent(CommandType command);
        public event CommandEvent SendCommand;

        private readonly HashSet<CommandType> _combinedCommands = new HashSet<CommandType>()
        {
            CommandType.LeftForward, CommandType.RightForward, CommandType.LeftBackward, CommandType.RightBackward
        };

        private readonly HashSet<CommandType> _speedControlCommands = new HashSet<CommandType>()
        {
            CommandType.HighSpeed, CommandType.MediumSpeed, CommandType.LowSpeed
        };

        private List<CommandType> _currentDirectionCommands = new List<CommandType>();

        public async Task OnDirectionCommandEnter(CommandType command)
        {
            if (_currentDirectionCommands.Contains(command))
            {
                return;
            }

            if (_currentDirectionCommands.Count == 0)
            {
                _currentDirectionCommands.Add(command);
                await SendCommand?.Invoke(command);
            }
            else
            {
                CommandType combinedCommand;
                if (_currentDirectionCommands.Count == 1 &&
                    TryGetCombinedCommand(_currentDirectionCommands[0], command, out combinedCommand))
                {
                    _currentDirectionCommands.Add(command);
                    await SendCommand?.Invoke(combinedCommand);
                }
                else
                {
                    _currentDirectionCommands.Clear();
                    _currentDirectionCommands.Add(command);
                    await SendCommand?.Invoke(command);
                }
            }
        }

        public async Task OnDirectionCommandExit(CommandType command)
        {
            if (!_currentDirectionCommands.Contains(command))
            {
                return;
            }

            _currentDirectionCommands.Remove(command);

            if (_currentDirectionCommands.Count == 0)
            {
                await SendCommand?.Invoke(CommandType.Stop);
            }
            else
            {
                await SendCommand?.Invoke(_currentDirectionCommands[0]);
            }
        }

        public async Task OnGeneralCommandEnter(CommandType command)
        {
            if (_speedControlCommands.Contains(command))
            {
                await OnSpeedControlCommand(command);
            }
            else
            {
                await OnDirectionCommandEnter(command);
            }
        }

        public async Task OnGeneralCommandExit(CommandType command)
        {
            if (_speedControlCommands.Contains(command))
            {
                return;
            }

            await OnDirectionCommandExit(command);
        }

        public async Task OnSpeedControlCommand(CommandType command)
        {
            await SendCommand?.Invoke(command);
        }

        private bool TryGetCombinedCommand(CommandType command1, CommandType command2, out CommandType combinedCommand)
        {
            return _combinedCommands.TryGetValue(command1 | command2, out combinedCommand);
        }
    }
}
