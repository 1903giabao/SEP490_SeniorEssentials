pipeline {
    agent any
    
    stages {
        stage('Retrieve Credentials') {
            steps {
                script {
                    withCredentials([
                        string(credentialsId: 'JwtSettings', variable: 'JwtSettings'),
                        string(credentialsId: 'SMSApiKey', variable: 'SMSApiKey'),
                        string(credentialsId: 'SMSSecretKey', variable: 'SMSSecretKey'),
                        string(credentialsId: 'SMSBrandName', variable: 'SMSBrandName'),
                        string(credentialsId: 'EmailServer', variable: 'EmailServer'),
                        string(credentialsId: 'EmailPort', variable: 'EmailPort'),
                        string(credentialsId: 'EmailUserName', variable: 'EmailUserName'),
                        string(credentialsId: 'EmailPassWord', variable: 'EmailPassWord'),
                        string(credentialsId: 'EmailUseSsl', variable: 'EmailUseSsl'),
                        string(credentialsId: 'EmailSenderEmail', variable: 'EmailSenderEmail'),
                        string(credentialsId: 'EmailSenderName', variable: 'EmailSenderName'),
                        string(credentialsId: 'Chattype', variable: 'Chattype'),
                        string(credentialsId: 'Chatproject_id', variable: 'Chatproject_id'),
                        string(credentialsId: 'Chatprivate_key_id', variable: 'Chatprivate_key_id'),
                        string(credentialsId: 'Chatprivate_key', variable: 'Chatprivate_key'),
                        string(credentialsId: 'Chatclient_email', variable: 'Chatclient_email'),
                        string(credentialsId: 'Chatclient_id', variable: 'Chatclient_id'),
                        string(credentialsId: 'Chatauth_uri', variable: 'Chatauth_uri'),
                        string(credentialsId: 'Chattoken_uri', variable: 'Chattoken_uri'),
                        string(credentialsId: 'Chatauth_provider_x509_cert_url', variable: 'Chatauth_provider_x509_cert_url'),
                        string(credentialsId: 'Chatclient_x509_cert_url', variable: 'Chatclient_x509_cert_url'),
                        string(credentialsId: 'Chatuniverse_domain', variable: 'Chatuniverse_domain')
                    ]) {
                        env.JwtSettings = JwtSettings
                        env.SMSApiKey = SMSApiKey
                        env.SMSSecretKey = SMSSecretKey
                        env.SMSBrandName = SMSBrandName
                        env.EmailServer = EmailServer
                        env.EmailPort = EmailPort
                        env.EmailUserName = EmailUserName
                        env.EmailPassWord = EmailPassWord
                        env.EmailUseSsl = EmailUseSsl
                        env.EmailSenderEmail = EmailSenderEmail
                        env.EmailSenderName = EmailSenderName
                        env.Chattype = Chattype
                        env.Chatproject_id = Chatproject_id
                        env.Chatprivate_key_id = Chatprivate_key_id
                        env.Chatprivate_key = Chatprivate_key
                        env.Chatclient_email = Chatclient_email
                        env.Chatclient_id = Chatclient_id
                        env.Chatauth_uri = Chatauth_uri
                        env.Chattoken_uri = Chattoken_uri
                        env.Chatauth_provider_x509_cert_url = Chatauth_provider_x509_cert_url
                        env.Chatclient_x509_cert_url = Chatclient_x509_cert_url
                        env.Chatuniverse_domain = Chatuniverse_domain
                    }
                }
            }
        }
        
        stage('Packaging') {
            steps {
                sh 'docker build --pull --rm -f Dockerfile -t senioressentials:latest .'
            }
        }

        stage('Push to DockerHub') {
            steps {
                withDockerRegistry(credentialsId: 'dockerhub', url: 'https://index.docker.io/v1/') {
                    sh 'docker tag senioressentials:latest senioressntials/senioressentials:latest'
                    sh 'docker push senioressntials/senioressentials:latest'
                }
            }
        }

        stage('Deploy BE to DEV') {
            steps {
                echo 'Deploying and cleaning'
                sh 'if [ $(docker ps -q -f name=senioressentials) ]; then docker container stop senioressentials; fi'
                sh 'echo y | docker system prune'
                sh 'docker container run -d --name senioressentials -p 8080:8080 -p 8081:8081 '+
                   '-e JwtSettings=${JwtSettings} ' +
                   '-e SMSApiKey=${SMSApiKey} '+
                   '-e SMSSecretKey=${SMSSecretKey} ' +
                   '-e SMSBrandName=${SMSBrandName} '+
                   '-e EmailServer=${EmailServer} ' +
                   '-e EmailPort=${EmailPort} ' +
                   '-e EmailUserName=${EmailUserName} ' +
                   '-e EmailPassWord=${EmailPassWord} ' +
                   '-e EmailUseSsl=${EmailUseSsl} ' +
                   '-e EmailSenderEmail=${EmailSenderEmail} '+
                   '-e EmailSenderName=${EmailSenderName} ' +
                   '-e Chattype=${Chattype} '+
                   '-e Chatproject_id=${Chatproject_id} '+
                   '-e Chatprivate_key_id=${Chatprivate_key_id} '+
                   '-e Chatprivate_key=${Chatprivate_key} '+
                   '-e Chatclient_email=${Chatclient_email} '+
                   '-e Chatclient_id=${Chatclient_id} '+
                   '-e Chatauth_uri=${Chatauth_uri} '+
                   '-e Chattoken_uri=${Chattoken_uri} '+
                   '-e Chatauth_provider_x509_cert_url=${Chatauth_provider_x509_cert_url} '+
                   '-e Chatclient_x509_cert_url=${Chatclient_x509_cert_url} '+
                   '-e Chatuniverse_domain=${Chatuniverse_domain} '+
                   'senioressntials/senioressentials:latest'
                
            }
        }
    }

    post {
        always {
            cleanWs()
        }
    }
}
