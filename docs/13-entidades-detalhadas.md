# Entidades Detalhadas

## Student

Representa o aluno acompanhado pela instituição.

| Campo         | Tipo        | Obrigatório |
| ------------- | ----------- | ----------- |
| Id            | Guid        | Sim         |
| FullName      | string(200) | Sim         |
| BirthDate     | DateTime    | Sim         |
| CPF           | string(14)  | Sim         |
| Phone         | string(20)  | Sim         |
| Email         | string(200) | Não         |
| Address       | string(500) | Não         |
| Status        | int         | Sim         |
| AdmissionDate | DateTime    | Sim         |
| CreatedAt     | DateTime    | Sim         |
| UpdatedAt     | DateTime    | Não         |

Relacionamentos:

* 1:N FamilyMembers
* 1:N Appointments
* 1:N MedicalEvolutions
* 1:N Payments
* 1:N StudentProgress

---

## FamilyMember

Representa familiares ou responsáveis.

| Campo           | Tipo        |
| --------------- | ----------- |
| Id              | Guid        |
| StudentId       | Guid        |
| FullName        | string(200) |
| Relationship    | string(100) |
| Phone           | string(20)  |
| Email           | string(200) |
| CanAccessPortal | bool        |

---

## Professional

Representa profissionais da instituição.

| Campo              | Tipo        |
| ------------------ | ----------- |
| Id                 | Guid        |
| FullName           | string(200) |
| Specialty          | string(100) |
| RegistrationNumber | string(100) |
| Email              | string(200) |
| Phone              | string(20)  |

---

## MedicalRecord

Prontuário principal do aluno.

| Campo         | Tipo     |
| ------------- | -------- |
| Id            | Guid     |
| StudentId     | Guid     |
| Anamnesis     | text     |
| ClinicalNotes | text     |
| CreatedAt     | DateTime |

---

## MedicalEvolution

Registro de evolução.

| Campo          | Tipo     |
| -------------- | -------- |
| Id             | Guid     |
| StudentId      | Guid     |
| ProfessionalId | Guid     |
| Description    | text     |
| CreatedAt      | DateTime |

---

## Appointment

Atendimento ou agendamento.

| Campo          | Tipo     |
| -------------- | -------- |
| Id             | Guid     |
| StudentId      | Guid     |
| ProfessionalId | Guid     |
| ScheduledAt    | DateTime |
| Status         | int      |
| Notes          | text     |

---

## Course

Curso EAD.

| Campo       | Tipo        |
| ----------- | ----------- |
| Id          | Guid        |
| Title       | string(200) |
| Description | text        |
| IsActive    | bool        |

---

## CourseModule

Módulo de curso.

| Campo    | Tipo        |
| -------- | ----------- |
| Id       | Guid        |
| CourseId | Guid        |
| Title    | string(200) |
| Order    | int         |

---

## Lesson

Aula do curso.

| Campo             | Tipo        |
| ----------------- | ----------- |
| Id                | Guid        |
| CourseModuleId    | Guid        |
| Title             | string(200) |
| VideoExternalId   | string      |
| DurationInMinutes | int         |

---

## Payment

Pagamento realizado.

| Campo         | Tipo     |
| ------------- | -------- |
| Id            | Guid     |
| StudentId     | Guid     |
| Amount        | decimal  |
| Status        | int      |
| PaymentMethod | int      |
| PaidAt        | DateTime |
