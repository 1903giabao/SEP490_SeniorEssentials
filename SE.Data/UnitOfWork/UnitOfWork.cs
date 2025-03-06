using SE.Data.Models;
using SE.Data.Repository;

using System;
using System.Threading.Tasks;

namespace SE.Data.UnitOfWork
{
    public class UnitOfWork
    {
        public SeniorEssentialsContext _unitOfWorkContext;

        private AccountRepository _accountRepository;
        private ActivityRepository _activityRepository;
        private ActivityScheduleRepository _activityScheduleRepository;
        private BookingRepository _bookingRepository;
        private ComboRepository _comboRepository;
        private ContentProviderRepository _contentProviderRepository;
        private EmergencyInformationRepository _emergencyInformationRepository;
        private ElderlyRepository _elderlyRepository;
        private FamilyMemberRepository _familyMemberRepository;
        private GameRepository _gameRepository;
        private GroupRepository _groupRepository;
        private GroupMemberRepository _groupMemberRepository;
        private IotdeviceRepository _idoeviceRepository;
        private LessonRepository _lessonRepository;
        private LessonHistoryRepository _lessonHistoryRepository;
        private LessonFeedbackRepository _lessonFeedbackRepository;
        private MedicationRepository _medicationRepository;
        private MedicationScheduleRepository _medicationScheduleRepository;
        private NotificationRepository _notificationRepository;
        private ProfessorAppointmentRepository _professorAppointmentRepository;
        private ProfessorRatingRepository _professorRatingRepository;
        private ProfessorScheduleRepository _professorScheduleRepository;
        private ProfessorRepository _professorRepository;
        private RoleRepository _roleRepository;
        private TimeSlotRepository _timeSlotRepository;
        private TransactionRepository _transactionRepository;
        private UserServiceRepository _userServiceRepository;
        private PrescriptionRepository _prescriptionRepository;
        private UserLinkRepository _userLinkRepository;
        private BloodPressureRepository _bloodPressureRepository;
        private BloodGlucoseRepository _bloodGlucoseRepository;
        private HeartRateRepository _heartRateRepository;
        private KidneyFunctionRepository _kidneyFunctionRepository;
        private LipidProfileRepository _lipidProfileRepository;
        private LiverEnzymeRepository _liverEnzymeRepository;
        private WeightRepository _weightRepository;
        private HeightRepository _heightRepository;


        public UnitOfWork()
        {
            _unitOfWorkContext ??= new SeniorEssentialsContext();
        }

        public UnitOfWork(SeniorEssentialsContext unitOfWorkContext)
        {
            _unitOfWorkContext = unitOfWorkContext ?? throw new ArgumentNullException(nameof(unitOfWorkContext));
        }

        public AccountRepository AccountRepository
        {
            get
            {
                return _accountRepository ??= new AccountRepository(_unitOfWorkContext);
            }
        }

        public PrescriptionRepository PrescriptionRepository
        {
            get
            {
                return _prescriptionRepository ??= new PrescriptionRepository(_unitOfWorkContext);
            }
        }
        public ActivityRepository ActivityRepository
        {
            get => _activityRepository ??= new ActivityRepository(_unitOfWorkContext);
        }

        public ActivityScheduleRepository ActivityScheduleRepository
        {
            get => _activityScheduleRepository ??= new ActivityScheduleRepository(_unitOfWorkContext);
        }

        public BookingRepository BookingRepository
        {
            get => _bookingRepository ??= new BookingRepository(_unitOfWorkContext);
        }

        public ComboRepository ComboRepository
        {
            get => _comboRepository ??= new ComboRepository(_unitOfWorkContext);
        }

        public ContentProviderRepository ContentProviderRepository
        {
            get => _contentProviderRepository ??= new ContentProviderRepository(_unitOfWorkContext);
        }

        public EmergencyInformationRepository EmergencyInformationRepository
        {
            get => _emergencyInformationRepository ??= new EmergencyInformationRepository(_unitOfWorkContext);
        }

        public ElderlyRepository ElderlyRepository
        {
            get => _elderlyRepository ??= new ElderlyRepository(_unitOfWorkContext);
        }

        public FamilyMemberRepository FamilyMemberRepository
        {
            get => _familyMemberRepository ??= new FamilyMemberRepository(_unitOfWorkContext);
        }

        public GameRepository GameRepository
        {
            get => _gameRepository ??= new GameRepository(_unitOfWorkContext);
        }

        public GroupRepository GroupRepository
        {
            get => _groupRepository ??= new GroupRepository(_unitOfWorkContext);
        }

        public GroupMemberRepository GroupMemberRepository
        {
            get => _groupMemberRepository ??= new GroupMemberRepository(_unitOfWorkContext);
        }
        public IotdeviceRepository IotdeviceRepository
        {
            get => _idoeviceRepository ??= new IotdeviceRepository(_unitOfWorkContext);
        }

        public LessonRepository LessonRepository
        {
            get => _lessonRepository ??= new LessonRepository(_unitOfWorkContext);
        }

        public LessonHistoryRepository LessonHistoryRepository
        {
            get => _lessonHistoryRepository ??= new LessonHistoryRepository(_unitOfWorkContext);
        }

        public LessonFeedbackRepository LessonFeedbackRepository
        {
            get => _lessonFeedbackRepository ??= new LessonFeedbackRepository(_unitOfWorkContext);
        }

        public MedicationRepository MedicationRepository
        {
            get => _medicationRepository ??= new MedicationRepository(_unitOfWorkContext);
        }

        public MedicationScheduleRepository MedicationScheduleRepository
        {
            get => _medicationScheduleRepository ??= new MedicationScheduleRepository(_unitOfWorkContext);
        }

        public NotificationRepository NotificationRepository
        {
            get => _notificationRepository ??= new NotificationRepository(_unitOfWorkContext);
        }

        public ProfessorAppointmentRepository ProfessorAppointmentRepository
        {
            get => _professorAppointmentRepository ??= new ProfessorAppointmentRepository(_unitOfWorkContext);
        }

        public ProfessorRatingRepository ProfessorRatingRepository
        {
            get => _professorRatingRepository ??= new ProfessorRatingRepository(_unitOfWorkContext);
        }

        public ProfessorScheduleRepository ProfessorScheduleRepository
        {
            get => _professorScheduleRepository ??= new ProfessorScheduleRepository(_unitOfWorkContext);
        }
        public ProfessorRepository ProfessorRepository
        {
            get => _professorRepository ??= new ProfessorRepository(_unitOfWorkContext);
        }
        public RoleRepository RoleRepository
        {
            get => _roleRepository ??= new RoleRepository(_unitOfWorkContext);
        }
        public TimeSlotRepository TimeSlotRepository
        {
            get => _timeSlotRepository ??= new TimeSlotRepository(_unitOfWorkContext);
        }
        public TransactionRepository TransactionRepository
        {
            get => _transactionRepository ??= new TransactionRepository(_unitOfWorkContext);
        }
        public UserServiceRepository UserServiceRepository
        {
            get => _userServiceRepository ??= new UserServiceRepository(_unitOfWorkContext);
        }

        public UserLinkRepository UserLinkRepository
        {
            get => _userLinkRepository ??= new UserLinkRepository(_unitOfWorkContext);
        }

        public BloodGlucoseRepository BloodGlucoseRepository
        {
            get => _bloodGlucoseRepository ??= new BloodGlucoseRepository(_unitOfWorkContext);
        }

        public BloodPressureRepository BloodPressureRepository
        {
            get => _bloodPressureRepository ??= new BloodPressureRepository(_unitOfWorkContext);
        }

        public HeartRateRepository HeartRateRepository
        {
            get => _heartRateRepository ??= new HeartRateRepository(_unitOfWorkContext);
        }

        public KidneyFunctionRepository KidneyFunctionRepository
        {
            get => _kidneyFunctionRepository ??= new KidneyFunctionRepository(_unitOfWorkContext);
        }

        public LipidProfileRepository LipidProfileRepository
        {
            get => _lipidProfileRepository ??= new LipidProfileRepository(_unitOfWorkContext);
        }

        public LiverEnzymeRepository LiverEnzymeRepository
        {
            get => _liverEnzymeRepository ??= new LiverEnzymeRepository(_unitOfWorkContext);
        }

        public WeightRepository WeightRepository
        {
            get => _weightRepository ??= new WeightRepository(_unitOfWorkContext);
        }        
        
        public HeightRepository HeightRepository
        {
            get => _heightRepository ??= new HeightRepository(_unitOfWorkContext);
        }

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