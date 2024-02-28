def find_interval_lengths(sequence):
    intervals = []
    current_count = 1

    for i in range(1, len(sequence)):
        if sequence[i] == sequence[i - 1]:
            current_count += 1
        else:
            intervals.append(current_count)
            current_count = 1

    # Add the count of the last interval
    intervals.append(current_count)

    return intervals


# Read data from a file
file_path = 'vm/data/data.txt'  # Replace with the actual file path
with open(file_path, 'r') as file:
    data = [int(line.strip()) for line in file]

# Example sequence
sequence = [1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 3]

# Find the lengths of each interval
interval_lengths = find_interval_lengths(data)

# Display the result
print("Lengths of each interval:", sorted(interval_lengths))
