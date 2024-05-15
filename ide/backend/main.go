package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"
	"os/exec"
)

const (
	INPUT_FILENAME  = "bin/in.txt"
	OUTPUT_FILENAME = "bin/out.txt"
)

type InputData struct {
	ConsoleInput string `json:"consoleInput"`
	EditorInput  string `json:"editorInput"`
}

func runCode(data InputData) string {
	// Запись содержимого data.ConsoleInput в файл in.txt
	err := os.WriteFile(INPUT_FILENAME, []byte(data.ConsoleInput), 0644)
	if err != nil {
		log.Printf("Ошибка при записи в файл in.txt: %v", err)
	}

	// Запись содержимого data.EditorInput в файл main.radi
	err = os.WriteFile("bin/main.radi", []byte(data.EditorInput), 0644)
	if err != nil {
		log.Printf("Ошибка при записи в файл main.radi: %v", err)
	}

	// Выполнение команды cat in.txt | ./radi main.radi > out.txt
	cmd := exec.Command("sh", "-c", "cat bin/in.txt | ./bin/radi bin/main.radi > bin/out.txt")
	err = cmd.Run()
	if err != nil {
		log.Printf("Ошибка при выполнении команды: %v", err)
		return fmt.Sprintf("Ошибка компиляции: %v", err)
	}

	// Чтение данных из файла out.txt и сохранение их в переменную ans
	outData, err := os.ReadFile(OUTPUT_FILENAME)
	if err != nil {
		log.Printf("Ошибка при чтении файла out.txt: %v", err)
	}

	return string(outData)
}

func handleRequest(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "text/html; charset=utf-8")
	w.Header().Set("Access-Control-Allow-Origin", "*")

	if r.Method != http.MethodPost {
		http.Error(w, "Only POST requests are supported", http.StatusMethodNotAllowed)
		return
	}

	var inputData InputData
	err := json.NewDecoder(r.Body).Decode(&inputData)
	if err != nil {
		http.Error(w, "Failed to decode JSON input", http.StatusBadRequest)
		return
	}

	// Можно использовать данные inputData
	fmt.Println("Received console input:", inputData.ConsoleInput)
	fmt.Println("Received editor input:", inputData.EditorInput)

	ans := runCode(inputData)

	// Отправляем ответ клиенту
	response := map[string]string{"message": ans}
	w.Header().Set("Content-Type", "application/json")
	err = json.NewEncoder(w).Encode(response)
	if err != nil {
		http.Error(w, "Failed to encode JSON response", http.StatusInternalServerError)
		return
	}
}

func main() {
	http.HandleFunc("/", handleRequest)

	fmt.Println("Server is running on port 8080...")
	log.Fatal(http.ListenAndServe(":8080", nil))
}
