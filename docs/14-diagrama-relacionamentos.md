# Diagrama de Relacionamentos

## CRM

```text
Student
│
├── FamilyMembers
├── MedicalRecord
├── MedicalEvolutions
├── Appointments
├── Payments
└── StudentProgress
```

## Profissionais

```text
Professional
│
├── Appointments
└── MedicalEvolutions
```

## Plataforma EAD

```text
Course
│
└── CourseModules
      │
      └── Lessons
               │
               └── StudentProgress
```

## Financeiro

```text
Student
│
├── Payments
└── Subscriptions
```

## Suporte

```text
Student
│
└── Tickets
        │
        └── Messages
```
