Feature: Create booking
  As a hotel guest
  I want to book a room for a period in the future
  So that I am sure to have a room when I arrive

  Requirement: a hotel room can be booked for a period (start date - end date)
  in the future, if it is not already booked for one or more days during the
  desired period.

  "day 10" means 10 days from today, and "day 0" is today.
  The test case IDs (TC1-TC10, V1-V6, A6, A7, A10) come from the Step 1 guide.

  Rule: A room can only be booked if it is free on every day of the period

    Background:
      Given the hotel has 2 rooms
      And all rooms are booked from day 10 to day 20

    Scenario Outline: Decision table rule
      When I book a room from day <start> to day <end>
      Then the booking is <result>
      And the number of new bookings is <saved>

      Examples: Decision table (B = before, O = occupied, A = after)
        | id   | SD | ED | start | end | result    | saved |
        | TC1  | B  | B  | 1     | 9   | confirmed | 1     |
        | TC2  | A  | A  | 21    | 21  | confirmed | 1     |
        | TC3  | B  | A  | 9     | 21  | declined  | 0     |
        | TC4  | B  | O  | 9     | 10  | declined  | 0     |
        | TC5  | B  | O  | 9     | 20  | declined  | 0     |
        | TC6  | O  | A  | 10    | 21  | declined  | 0     |
        | TC7  | O  | A  | 20    | 21  | declined  | 0     |
        | TC8  | O  | O  | 10    | 10  | declined  | 0     |
        | TC9  | O  | O  | 10    | 20  | declined  | 0     |
        | TC10 | O  | O  | 20    | 20  | declined  | 0     |

    Scenario: A10 each booking uses up one free room
      When I book a room from day 1 to day 2
      Then the booking is confirmed
      When I book a room from day 1 to day 2
      Then the booking is confirmed
      When I book a room from day 1 to day 2
      Then the booking is declined

    @ignore @defect_D-2
    Scenario: A7 a start time on the last occupied day is still occupied
      When I book a room from day 20 at "12:00" to day 21
      Then the booking is declined
      And the number of new bookings is 0

  Rule: The period must start after today and must not end before it starts

    Background:
      Given the hotel has 1 room
      And there are no bookings

    Scenario Outline: Invalid dates are rejected
      When I book a room from day <start> to day <end>
      Then the dates are rejected as invalid
      And the number of new bookings is 0

      Examples: Equivalence classes and boundary values
        | id | description             | start | end |
        | V1 | start is today          | 0     | 5   |
        | V2 | start is yesterday      | -1    | 5   |
        | V3 | whole period in past    | -5    | -1  |
        | V5 | start one day after end | 6     | 5   |
        | V6 | start in A, end in B    | 21    | 9   |

    Scenario: V4 start tomorrow, one-day stay
      When I book a room from day 1 to day 1
      Then the booking is confirmed
      And the number of new bookings is 1

    @ignore @defect_D-1
    Scenario: A6 start today with a time of day
      When I book a room from day 0 at "00:00:01" to day 2
      Then the dates are rejected as invalid
      And the number of new bookings is 0
