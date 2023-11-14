pipeline{
    agent any
    triggers{
        pollSCM("* * * * *")
    }
    stages{
        stage('Build') {
            steps {
                sh "/usr/local/bin/docker compose build"
            }
        }
      

        stage('Deploy') {
            steps {
                sh "/usr/local/bin/docker compose up --build web-ui"
            }
        }
    }
}
