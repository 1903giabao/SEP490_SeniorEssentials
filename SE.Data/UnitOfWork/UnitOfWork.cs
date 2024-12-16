using SE.Data.Models;
using SE.Data.Repository;

using System;
using System.Threading.Tasks;

namespace SE.Data.UnitOfWork
{
    public class UnitOfWork
    {
        private readonly SeniorEssentialsContext _unitOfWorkContext;

        private AccountRepository _accountRepository;
        private ActivityRepository _activityRepository;
        private ActivityScheduleRepository _activityScheduleRepository;
        private ChatRepository _chatRepository;
        private EmergencyContactRepository _emergencyContactRepository;
        private ElderlyRepository _elderlyRepository;
        private GameRepository _gameRepository;
        private GroupRepository _groupRepository;
        private GroupMemberRepository _groupMemberRepository;
        private HealthIndicatorRepository _healthIndicatorRepository;
        private IotdeviceRepository _idoeviceRepository;
        private LessonFeedbackRepository _lessonFeedbackRepository;
        private MedicationHistoryRepository _medicationHistoryRepository;
        private MedicationScheduleRepository _medicationScheduleRepository;
        private MessageRepository _messageRepository;
        private NotificationRepository _notificationRepository;
        private ProfessorAppointmentRepository _professorAppointmentRepository;
        private ProfessorRatingRepository _professorRatingRepository;
        private ProfessorScheduleRepository _professorScheduleRepository;
        private RoleRepository _roleRepository;
        private VideoCallRepository _videoCallRepository;

        public UnitOfWork(SeniorEssentialsContext unitOfWorkContext)
        {
            _unitOfWorkContext = unitOfWorkContext ?? throw new ArgumentNullException(nameof(unitOfWorkContext));
        }

        public AccountRepository AccountRepository => _accountRepository ??= new AccountRepository(_unitOfWorkContext);
        public ActivityRepository ActivityRepository => _activityRepository ??= new ActivityRepository(_unitOfWorkContext);
        public ActivityScheduleRepository ActivityScheduleRepository => _activityScheduleRepository ??= new ActivityScheduleRepository(_unitOfWorkContext);
        public ChatRepository ChatRepository => _chatRepository ??= new ChatRepository(_unitOfWorkContext);
        public EmergencyContactRepository EmergencyContactRepository => _emergencyContactRepository ??= new EmergencyContactRepository(_unitOfWorkContext);
        public ElderlyRepository ElderlyRepository => _elderlyRepository ??= new ElderlyRepository(_unitOfWorkContext);
        public GameRepository GameRepository => _gameRepository ??= new GameRepository(_unitOfWorkContext);
        public GroupRepository GroupRepository => _groupRepository ??= new GroupRepository(_unitOfWorkContext);
        public GroupMemberRepository GroupMemberRepository => _groupMemberRepository ??= new GroupMemberRepository(_unitOfWorkContext);
        public HealthIndicatorRepository HealthIndicatorRepository => _healthIndicatorRepository ??= new HealthIndicatorRepository(_unitOfWorkContext);
        public IotdeviceRepository IdoeviceRepository => _idoeviceRepository ??= new IotdeviceRepository(_unitOfWorkContext);
        public LessonFeedbackRepository LessonFeedbackRepository => _lessonFeedbackRepository ??= new LessonFeedbackRepository(_unitOfWorkContext);
        public MedicationHistoryRepository MedicationHistoryRepository => _medicationHistoryRepository ??= new MedicationHistoryRepository(_unitOfWorkContext);
        public MedicationScheduleRepository MedicationScheduleRepository => _medicationScheduleRepository ??= new MedicationScheduleRepository(_unitOfWorkContext);
        public MessageRepository MessageRepository => _messageRepository ??= new MessageRepository(_unitOfWorkContext);
        public NotificationRepository NotificationRepository => _notificationRepository ??= new NotificationRepository(_unitOfWorkContext);
        public ProfessorAppointmentRepository ProfessorAppointmentRepository => _professorAppointmentRepository ??= new ProfessorAppointmentRepository(_unitOfWorkContext);
        public ProfessorRatingRepository ProfessorRatingRepository => _professorRatingRepository ??= new ProfessorRatingRepository(_unitOfWorkContext);
        public ProfessorScheduleRepository ProfessorScheduleRepository => _professorScheduleRepository ??= new ProfessorScheduleRepository(_unitOfWorkContext);
        public RoleRepository RoleRepository => _roleRepository ??= new RoleRepository(_unitOfWorkContext);
        public VideoCallRepository VideoCallRepository => _videoCallRepository ??= new VideoCallRepository(_unitOfWorkContext);

        public int SaveChangesWithTransaction()
        {
            int result = -1;

            using (var dbContextTransaction = _unitOfWorkContext.Database.BeginTransaction())
            {
                try
                {
                    result = _unitOfWorkContext.SaveChanges();
                    dbContextTransaction.Commit();
                }
                catch (Exception)
                {
                    result = -1;
                    dbContextTransaction.Rollback();
                }
            }

            return result;
        }

        public async Task<int> SaveChangesWithTransactionAsync()
        {
            int result = -1;

            using (var dbContextTransaction = _unitOfWorkContext.Database.BeginTransaction())
            {
                try
                {
                    result = await _unitOfWorkContext.SaveChangesAsync();
                    dbContextTransaction.Commit();
                }
                catch (Exception)
                {
                    result = -1;
                    dbContextTransaction.Rollback();
                }
            }

            return result;
        }
    }
}