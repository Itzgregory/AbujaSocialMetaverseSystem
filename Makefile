.PHONY: build run clean test restore

# Default target
all: build

# Delegate build to the backend folder
build:
	$(MAKE) -C backend build

# Delegate run to the backend folder
run:
	$(MAKE) -C backend run

# Delegate clean to the backend folder
clean:
	$(MAKE) -C backend clean

# Delegate test to the backend folder
test:
	$(MAKE) -C backend test

# Delegate restore to the backend folder
restore:
	$(MAKE) -C backend restore
