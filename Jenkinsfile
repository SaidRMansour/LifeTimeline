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
      

        
    }
}
