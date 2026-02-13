window.uiPhysics = {
    createBurst: function (x, y, color, count = 20) {
        for (let i = 0; i < count; i++) {
            const particle = document.createElement('div');
            particle.className = 'ui-particle';
            particle.style.backgroundColor = color;
            
            // Random direction and velocity
            const angle = Math.random() * Math.PI * 2;
            const velocity = 5 + Math.random() * 15;
            const vx = Math.cos(angle) * velocity;
            const vy = Math.sin(angle) * velocity;
            
            particle.style.left = x + 'px';
            particle.style.top = y + 'px';
            
            document.body.appendChild(particle);
            
            let posx = x;
            let posy = y;
            let opacity = 1;
            
            const animate = () => {
                posx += vx;
                posy += vy;
                opacity -= 0.02;
                
                particle.style.transform = `translate(${posx - x}px, ${posy - y}px) scale(${opacity})`;
                particle.style.opacity = opacity;
                
                if (opacity > 0) {
                    requestAnimationFrame(animate);
                } else {
                    particle.remove();
                }
            };
            
            requestAnimationFrame(animate);
        }
    }
};
