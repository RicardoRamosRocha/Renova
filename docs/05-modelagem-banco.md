# Modelagem Inicial do Banco de Dados

## Entidades Principais

### ApplicationUser

Representa os usuários do sistema.

Campos principais:

* Id
* FullName
* Email
* PhoneNumber
* IsActive
* CreatedAt

### Student

Representa o aluno acompanhado pela instituição.

Campos principais:

* Id
* FullName
* BirthDate
* CPF
* Phone
* Email
* Address
* Status
* AdmissionDate
* CreatedAt

### FamilyMember

Representa o familiar ou responsável pelo aluno.

Campos principais:

* Id
* StudentId
* FullName
* Relationship
* Phone
* Email
* CanAccessPortal
* CreatedAt

### Professional

Representa profissionais internos da instituição.

Campos principais:

* Id
* FullName
* Specialty
* ProfessionalRegistration
* Phone
* Email
* IsActive

### MedicalRecord

Representa o prontuário do aluno.

Campos principais:

* Id
* StudentId
* Anamnesis
* ClinicalNotes
* CreatedAt
* UpdatedAt

### MedicalEvolution

Representa uma evolução registrada no prontuário.

Campos principais:

* Id
* StudentId
* ProfessionalId
* Description
* CreatedAt

### Appointment

Representa um agendamento.

Campos principais:

* Id
* StudentId
* ProfessionalId
* ScheduledAt
* Status
* Notes

### Course

Representa um curso EAD.

Campos principais:

* Id
* Title
* Description
* IsActive
* CreatedAt

### CourseModule

Representa um módulo dentro de um curso.

Campos principais:

* Id
* CourseId
* Title
* Description
* Order

### Lesson

Representa uma aula.

Campos principais:

* Id
* CourseModuleId
* Title
* Description
* VideoProvider
* VideoExternalId
* DurationInMinutes
* Order

### StudentProgress

Representa o progresso do aluno nas aulas.

Campos principais:

* Id
* StudentId
* LessonId
* WatchedPercentage
* CompletedAt

### Payment

Representa pagamentos.

Campos principais:

* Id
* StudentId
* Amount
* Status
* PaymentMethod
* ExternalPaymentId
* PaidAt
* CreatedAt

### Subscription

Representa assinaturas recorrentes.

Campos principais:

* Id
* StudentId
* PlanName
* Amount
* Status
* NextDueDate
* CreatedAt
