Feature: Fully occupied dates
  As a hotel receptionist
  I want to see the dates where every room is booked
  So that I know which dates I cannot offer to guests

  A date is fully occupied when every room has an active booking on that date.

  Background:
    Given the hotel has 2 rooms

  Scenario: Only the days where every room is booked are fully occupied
    Given room 1 is booked from day 10 to day 20
    And room 2 is booked from day 15 to day 25
    When I ask for the fully occupied dates from day 1 to day 30
    Then the fully occupied dates are day 15 to day 20

  Scenario: A cancelled booking does not make a date fully occupied
    Given room 1 is booked from day 10 to day 20
    And room 2 has a cancelled booking from day 10 to day 20
    When I ask for the fully occupied dates from day 1 to day 30
    Then the fully occupied dates are none

  Scenario Outline: Only dates inside the period I ask about are returned
    Given all rooms are booked from day 10 to day 20
    When I ask for the fully occupied dates from day <from> to day <to>
    Then the fully occupied dates are <dates>

    Examples:
      | from | to | dates            |
      | 1    | 9  | none             |
      | 1    | 10 | day 10 to day 10 |
      | 10   | 20 | day 10 to day 20 |
      | 12   | 15 | day 12 to day 15 |
      | 20   | 25 | day 20 to day 20 |
      | 21   | 30 | none             |
      | 1    | 30 | day 10 to day 20 |
