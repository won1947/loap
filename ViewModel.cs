using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace LostArkPersonnelDistributor
{
    public class ViewModel : INotifyPropertyChanged
    {
        private const int MaxPlayers = 16;  // 최대 16명의 플레이어
        private int _playerCount;  // 현재 추가된 플레이어 수

        // 각 역할의 인원 카운트
        private int _mainDPSCount;
        private int _subDPSCount;
        private int _mainSupportCount;
        private int _subSupportCount;

        // 부족한 캐릭터 수
        public int ShortageMainDPS { get; set; }
        public int ShortageSubDPS { get; set; }
        public int ShortageMainSupport { get; set; }
        public int ShortageSubSupport { get; set; }

        // 추천 메시지
        private string _recommendation;
        public string Recommendation
        {
            get { return _recommendation; }
            set
            {
                _recommendation = value;
                OnPropertyChanged(nameof(Recommendation));  // 속성 변경 알림
            }
        }

        // 초과된 캐릭터 수
        public int ExcessMainDPS { get; set; }
        public int ExcessSubDPS { get; set; }
        public int ExcessMainSupport { get; set; }
        public int ExcessSubSupport { get; set; }

        // 상태 메시지
        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));  // 속성 변경 알림
            }
        }

        // 플레이어 리스트
        public ObservableCollection<Player> Players { get; set; }

        // 명령어 정의
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CalculateCommand { get; }

        public ViewModel()
        {
            Players = new ObservableCollection<Player>();
            AddCommand = new RelayCommand<string>(AddCharacter, CanAddCharacter);
            DeleteCommand = new RelayCommand<string>(DeleteCharacter);
            CalculateCommand = new RelayCommand(CalculateRoles);

            _playerCount = 0;  // 초기 플레이어 수는 0
        }

        // 플레이어 추가 가능 여부 확인
        private bool CanAddCharacter(string characterType)
        {
            return _playerCount < MaxPlayers;
        }

        private void AddCharacter(string characterType)
        {
            if (_playerCount < MaxPlayers)
            {
                Players.Add(new Player(characterType));
                _playerCount++;
                UpdateCharacterCounts(characterType, true);  // 캐릭터 카운트 업데이트

                StatusMessage = $"플레이어 추가됨: {characterType} (현재 추가된 플레이어 수: {_playerCount}/{MaxPlayers})";
                OnPropertyChanged(nameof(StatusMessage)); // 상태 메시지 업데이트

                CommandManager.InvalidateRequerySuggested();  // 버튼 상태 갱신
            }
            else
            {
                StatusMessage = "더 이상 플레이어를 추가할 수 없습니다. 최대 16명까지 추가 가능합니다.";
                OnPropertyChanged(nameof(StatusMessage)); // 상태 메시지 업데이트
            }
        }

        private void DeleteCharacter(string characterType)
        {
            var playerToRemove = Players.FirstOrDefault(p => p.Type == characterType);
            if (playerToRemove != null)
            {
                Players.Remove(playerToRemove);
                _playerCount--;
                UpdateCharacterCounts(characterType, false);  // 캐릭터 카운트 감소

                StatusMessage = $"플레이어 삭제됨: {characterType} (현재 추가된 플레이어 수: {_playerCount}/{MaxPlayers})";
                OnPropertyChanged(nameof(StatusMessage)); // 상태 메시지 업데이트

                CommandManager.InvalidateRequerySuggested();  // 버튼 상태 갱신
            }
        }


        // 캐릭터 수 업데이트
        private void UpdateCharacterCounts(string characterType, bool increase)
        {
            int modifier = increase ? 1 : -1;

            switch (characterType)
            {
                case "딜딜":
                    _mainDPSCount += modifier;
                    _subDPSCount += modifier;
                    break;
                case "딜폿":
                    _mainDPSCount += modifier;
                    _subSupportCount += modifier;
                    break;
                case "폿딜":
                    _mainSupportCount += modifier;
                    _subDPSCount += modifier;
                    break;
                case "폿폿":
                    _mainSupportCount += modifier;
                    _subSupportCount += modifier;
                    break;
            }

            // 상태 메시지 업데이트
            StatusMessage = $"본캐 딜러: {_mainDPSCount}명, 부캐 딜러: {_subDPSCount}명\n" +
                            $"본캐 서포터: {_mainSupportCount}명, 부캐 서포터: {_subSupportCount}명";
        }

        // 부족 및 초과 인원 계산
        private void CalculateRoles()
        {
            // 두 판 기준 필요한 캐릭터 수
            const int requiredMainDPS = 6 * 2;   // 본캐 딜러 2판 기준
            const int requiredSubDPS = 6 * 2;    // 부캐 딜러 2판 기준
            const int requiredMainSupport = 2 * 2;  // 본캐 서포터 2판 기준
            const int requiredSubSupport = 2 * 2;   // 부캐 서포터 2판 기준

            // 부족 인원 계산
            ShortageMainDPS = Math.Max(0, requiredMainDPS - _mainDPSCount);
            ShortageSubDPS = Math.Max(0, requiredSubDPS - _subDPSCount);
            ShortageMainSupport = Math.Max(0, requiredMainSupport - _mainSupportCount);
            ShortageSubSupport = Math.Max(0, requiredSubSupport - _subSupportCount);

            // 초과 인원 계산
            ExcessMainDPS = Math.Max(0, _mainDPSCount - requiredMainDPS);
            ExcessSubDPS = Math.Max(0, _subDPSCount - requiredSubDPS);
            ExcessMainSupport = Math.Max(0, _mainSupportCount - requiredMainSupport);
            ExcessSubSupport = Math.Max(0, _subSupportCount - requiredSubSupport);

            // 결과 메시지 업데이트
            StatusMessage = $"본캐 딜러: {ShortageMainDPS}명 필요, 부캐 딜러: {ShortageSubDPS}명 필요\n" +
                            $"본캐 서포터: {ShortageMainSupport}명 필요, 부캐 서포터: {ShortageSubSupport}명 필요\n" +
                            $"초과된 본캐 딜러: {ExcessMainDPS}명, 부캐 딜러: {ExcessSubDPS}명\n" +
                            $"초과된 본캐 서포터: {ExcessMainSupport}명, 부캐 서포터: {ExcessSubSupport}명";

            // 추천 로직
            string recommendation = "";

            // 본캐 딜러가 부족하면 딜딜이나 딜폿 추천
            if (ShortageMainDPS > 0)
            {
                recommendation += "추천: 딜딜 (본캐 딜러) 또는 딜폿 (본캐 딜러 + 부캐 서포터)\n";
            }

            // 부캐 딜러가 부족하면 폿딜이나 딜딜 추천
            if (ShortageSubDPS > 0)
            {
                recommendation += "추천: 폿딜 (본캐 서포터 + 부캐 딜러) 또는 딜딜 (부캐 딜러)\n";
            }

            // 본캐 서포터가 부족하면 폿딜이나 폿폿 추천
            if (ShortageMainSupport > 0)
            {
                recommendation += "추천: 폿딜 (본캐 서포터 + 부캐 딜러) 또는 폿폿 (본캐 서포터)\n";
            }

            // 부캐 서포터가 부족하면 딜폿이나 폿폿 추천
            if (ShortageSubSupport > 0)
            {
                recommendation += "추천: 딜폿 (본캐 딜러 + 부캐 서포터) 또는 폿폿 (부캐 서포터)\n";
            }

            // 최종 추천 메시지 업데이트
            Recommendation = recommendation;
            OnPropertyChanged(nameof(Recommendation));  // 변경 알림
            OnPropertyChanged(nameof(StatusMessage));  // 상태 메시지 변경 알림
        }        // INotifyPropertyChanged 구현
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Player
    {
        public string Type { get; set; }

        public Player(string type)
        {
            Type = type;
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
