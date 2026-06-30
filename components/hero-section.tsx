'use client';

import Image from 'next/image';
import Link from 'next/link';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Event } from '@/lib/mock-data';

interface HeroSectionProps {
  featuredEvents: Event[];
}

export function HeroSection({ featuredEvents }: HeroSectionProps) {
  const [currentIndex, setCurrentIndex] = useState(0);

    useEffect(() => {
    if (featuredEvents.length === 0) return;
    const timer = setInterval(() => {
      setCurrentIndex((prev) => (prev + 1) % featuredEvents.length);
    }, 5000);
    return () => clearInterval(timer);
  }, [featuredEvents.length]);

  const goToPrevious = () => {
    setCurrentIndex((prev) => (prev - 1 + featuredEvents.length) % featuredEvents.length);
  };

  const goToNext = () => {
    setCurrentIndex((prev) => (prev + 1) % featuredEvents.length);
  };

  if (featuredEvents.length === 0) return null;

  const currentEvent = featuredEvents[currentIndex];

  return (
    <section className="relative overflow-hidden bg-gradient-to-b from-secondary/50 to-background">
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 sm:py-12 lg:px-8 lg:py-16">
        <div className="relative">
          {/* Main Featured Event */}
          <Link href={`/evento/${currentEvent.id}`} className="group block">
            <div className="relative aspect-[21/9] overflow-hidden rounded-2xl shadow-xl">
              <Image
                src={currentEvent.image}
                alt={currentEvent.title}
                fill
                className="object-cover transition-transform duration-500 group-hover:scale-105"
                priority
              />
              {/* Gradient Overlay */}
              <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/30 to-transparent" />
              
              {/* Content */}
              <div className="absolute bottom-0 left-0 right-0 p-6 sm:p-8 lg:p-10">
                <div className="mb-2 sm:mb-3">
                  <span className="inline-block rounded-full bg-primary px-3 py-1 text-xs font-medium text-primary-foreground sm:text-sm">
                    Em Destaque
                  </span>
                </div>
                <h2 className="mb-2 text-xl font-bold text-white sm:text-2xl md:text-3xl lg:text-4xl">
                  {currentEvent.title}
                </h2>
                <p className="mb-3 hidden text-sm text-white/80 sm:line-clamp-2 md:text-base lg:block">
                  {currentEvent.description}
                </p>
                <div className="flex flex-wrap items-center gap-4 text-sm text-white/90 sm:text-base">
                  <span>{currentEvent.date}</span>
                  <span className="hidden sm:inline">•</span>
                  <span className="hidden sm:inline">{currentEvent.location}</span>
                  <span className="hidden sm:inline">•</span>
                  <span className="hidden sm:inline">{currentEvent.city}</span>
                </div>
              </div>
            </div>
          </Link>

          {/* Navigation Arrows */}
          <Button
            variant="secondary"
            size="icon"
            className="absolute left-4 top-1/2 -translate-y-1/2 bg-white/90 shadow-lg hover:bg-white"
            onClick={goToPrevious}
          >
            <ChevronLeft className="h-5 w-5" />
          </Button>
          <Button
            variant="secondary"
            size="icon"
            className="absolute right-4 top-1/2 -translate-y-1/2 bg-white/90 shadow-lg hover:bg-white"
            onClick={goToNext}
          >
            <ChevronRight className="h-5 w-5" />
          </Button>

          {/* Dots Indicator */}
          <div className="mt-4 flex justify-center gap-2">
            {featuredEvents.map((_, index) => (
              <button
                key={index}
                className={`h-2 rounded-full transition-all ${
                  index === currentIndex 
                    ? 'w-8 bg-primary' 
                    : 'w-2 bg-muted-foreground/30 hover:bg-muted-foreground/50'
                }`}
                onClick={() => setCurrentIndex(index)}
              />
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
