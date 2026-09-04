# RaceDay Event Management System

## PROG6212 Programming 2B

RaceDay is a web-based event management system designed for South African road running, walking and cycling events.

The system supports two main user roles: Organisers and Participants. Organisers manage events, categories, enrolments and participant results, while Participants can register, browse events, enter events and view their results.

This repository contains the planning and database work completed for Part 1 of the PROG6212 Programming 2B PoE.

## System Purpose

RaceDay is planned as a centralised event management system for road running, walking and cycling events. The system is designed to help Organisers manage events and participant information while allowing Participants to register for events and track their results.

---

## Part 1

Part 1 focuses on system planning, database design and repository configuration.

The main deliverables are:

- Entity Relationship Diagram (ERD)
- API Endpoint Plan
- SQL Database Script
- GitHub repository
- GitHub Actions CI/CD validation

---

## User Roles

### Organiser

Organisers will be able to:

- Create events
- Edit events
- Delete events
- Manage event categories
- View participant enrolments
- Capture participant results
- View information about events they manage

### Participant

Participants will be able to:

- Register an account
- Log in
- Browse available events
- Enter an event
- Select an event category
- View their own enrolments
- View and track their race results and performance history

---

## Repository Structure

```text
RaceDay/
│
├── .github/
│   └── workflows/
│       └── build.yml
│
├── docs/
│   ├── RaceDay_Database.sql
│   ├── API Endpoint Plan
│   └── RaceDay ERD.pdf
│
└── README.md